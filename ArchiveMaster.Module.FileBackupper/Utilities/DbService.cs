using System.Diagnostics;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using ArchiveMaster.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.Utilities;

public class DbService : IDisposable, IAsyncDisposable
{
    private static readonly HashSet<BackupTask> initializedTasks = new HashSet<BackupTask>();

    private readonly BackupperDbContext db;

    public DbService(BackupTask backupTask)
    {
        BackupTask = backupTask;
        db = new BackupperDbContext(backupTask);
        Initialize();
    }

    public BackupTask BackupTask { get; }

    public void Add(object entity)
    {
        switch (entity)
        {
            case BackupSnapshotEntity snapshotEntity:
                db.Snapshots.Add(snapshotEntity);
                break;
            case FileRecordEntity record:
                db.Records.Add(record);
                break;
            case PhysicalFileEntity file:
                db.Files.Add(file);
                break;
            case BackupLogEntity log:
                db.Logs.Add(log);
                break;
            default:
                db.Add(entity);
                break;
        }
    }

    public void Dispose()
    {
        db?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (db != null)
        {
            await db.DisposeAsync();
        }
    }

    public IEnumerable<FileRecordEntity> GetLatestFiles(int snapshotId)
    {
        Initialize();
        BackupSnapshotEntity snapshot = GetValidSnapshots()
                                            .FirstOrDefault(p => p.Id == snapshotId)
                                        ?? throw new KeyNotFoundException(
                                            $"找不到ID为{snapshotId}的{nameof(BackupSnapshotEntity)}");
        return GetLatestFiles(snapshot);
    }

    public IEnumerable<FileRecordEntity> GetLatestFiles(BackupSnapshotEntity snapshot)
    {
        Initialize();
        BackupSnapshotEntity fullSnapshot;
        List<BackupSnapshotEntity> incrementalSnapshots = new List<BackupSnapshotEntity>();
        if (snapshot.IsFull)
        {
            fullSnapshot = snapshot;
        }
        else
        {
            fullSnapshot = GetValidSnapshots()
                               .Where(p => p.IsFull)
                               .Where(p => p.BeginTime < snapshot.BeginTime)
                               .OrderByDescending(p => p.BeginTime)
                               .FirstOrDefault()
                           ?? throw new KeyNotFoundException(
                               $"找不到该{nameof(BackupSnapshotEntity)}对应的全量备份{nameof(BackupSnapshotEntity)}");
            incrementalSnapshots.AddRange(GetValidSnapshots()
                .Where(p => !p.IsFull)
                .Where(p => p.BeginTime > fullSnapshot.EndTime)
                .Where(p => p.EndTime < snapshot.BeginTime)
                .OrderByDescending(p => p.BeginTime)
                .AsEnumerable());

            incrementalSnapshots.Add(snapshot);
        }

        var fileRecords = db.Records
            .Where(p => p.SnapshotId == fullSnapshot.Id)
            .Include(p => p.PhysicalFile)
            .AsEnumerable()
            .ToDictionary(p => p.RawFileRelativePath);


        foreach (var incrementalSnapshot in incrementalSnapshots)
        {
            var incrementalFiles = db.Records
                .Where(p => p.SnapshotId == incrementalSnapshot.Id)
                .Include(p => p.PhysicalFile)
                .AsEnumerable();
            foreach (var incrementalFile in incrementalFiles)
            {
                switch (incrementalFile.Type)
                {
                    case FileRecordType.Created:
                        if (fileRecords.ContainsKey(incrementalFile.RawFileRelativePath))
                        {
                            throw new Exception("增量备份中，文件被新增，但先前版本的文件中已存在该文件");
                        }

                        fileRecords.Add(incrementalFile.RawFileRelativePath, incrementalFile);
                        break;

                    case FileRecordType.Modified:
                        if (!fileRecords.ContainsKey(incrementalFile.RawFileRelativePath))
                        {
                            throw new Exception("增量备份中，文件被修改，但不能在先前版本的文件中找到这一个文件");
                        }

                        fileRecords[incrementalFile.RawFileRelativePath] = incrementalFile;
                        break;

                    case FileRecordType.Deleted:
                        if (!fileRecords.ContainsKey(incrementalFile.RawFileRelativePath))
                        {
                            throw new Exception("增量备份中，文件被删除，但不能在先前版本的文件中找到这一个文件");
                        }

                        fileRecords.Remove(incrementalFile.RawFileRelativePath);
                        break;
                }
            }
        }

        return fileRecords.Values;
    }

    public PhysicalFileEntity GetSameFile(DateTime time, long length, string sha1)
    {
        Initialize();
        var query = db.Files
            .Where(p => p.Time == time)
            .Where(p => p.Length == length);
        if (sha1 != null)
        {
            query = query.Where(p => p.Hash == sha1);
        }

        return query.FirstOrDefault();
    }

    public async Task<List<BackupSnapshotWithFileCount>> GetSnapshotsWithFilesAsync(bool? isFull = null,
        bool? isVirtual = null,
        CancellationToken token = default)
    {
        await InitializeAsync(token);
        var query = GetSnapshotsQuery(isFull, isVirtual).Select(s => new BackupSnapshotWithFileCount()
        {
            Snapshot = s,
            CreatedFileCount = db.Records.Count(p => p.Type == FileRecordType.Created && p.SnapshotId == s.Id),
            DeletedFileCount = db.Records.Count(p => p.Type == FileRecordType.Deleted && p.SnapshotId == s.Id),
            ModifiedFileCount = db.Records.Count(p => p.Type == FileRecordType.Modified && p.SnapshotId == s.Id),
        });
        return await query.ToListAsync(token).ConfigureAwait(false);
    }

    private IQueryable<BackupSnapshotEntity> GetSnapshotsQuery(bool? isFull, bool? isVirtual)
    {
        IQueryable<BackupSnapshotEntity> query = GetValidSnapshots();
        if (isFull.HasValue)
        {
            query = query.Where(p => p.IsFull == isFull);
        }

        if (isVirtual.HasValue)
        {
            query = query.Where(p => p.IsVirtual == isVirtual);
        }

        query = query.OrderBy(p => p.BeginTime);
        return query;
    }


    public async Task<List<BackupSnapshotEntity>> GetSnapshotsAsync(bool? isFull = null, bool? isVirtual = null,
        CancellationToken token = default)
    {
        await InitializeAsync(token);
        return await GetSnapshotsQuery(isFull, isVirtual).ToListAsync(token).ConfigureAwait(false);
    }

    public async Task LogAsync(LogLevel logLevel, string message, BackupSnapshotEntity snapshot = null,
        string detail = null)
    {
        await using var tempDb = new BackupperDbContext(BackupTask);
        await InitializeAsync(default, tempDb);
        BackupLogEntity log = new BackupLogEntity()
        {
            Message = message,
            Type = logLevel,
            SnapshotId = snapshot?.Id,
            Detail = detail
        };
        Debug.WriteLine($"{DateTime.Now}\t\t{message}");
        tempDb.Logs.Add(log);
        await tempDb.SaveChangesAsync();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<BackupLogEntity>> GetLogsAsync(int? snapshotId = null, LogLevel? type = null)
    {
        await InitializeAsync();
        IQueryable<BackupLogEntity> query = db.Logs;
        if (snapshotId.HasValue)
        {
            query = query.Where(p => p.SnapshotId == snapshotId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(p => p.Type == type.Value);
        }

        query = query.OrderBy(p => p.Time);
        return await query.ToListAsync();
    }

    private IQueryable<BackupSnapshotEntity> GetValidSnapshots()
    {
        return db.Snapshots
            .Where(p => p.EndTime != default)
            .Where(p => !p.IsDeleted);
    }

    private ValueTask InitializeAsync(CancellationToken cancellationToken = default, DbContext db = null)
    {
        if (initializedTasks.Contains(BackupTask))
        {
            return ValueTask.CompletedTask;
        }

        db ??= this.db;
        initializedTasks.Add(BackupTask);
        return new ValueTask(db.Database.EnsureCreatedAsync(cancellationToken));
    }

    private void Initialize(DbContext db = null)
    {
        if (initializedTasks.Contains(BackupTask))
        {
            return;
        }

        db ??= this.db;
        initializedTasks.Add(BackupTask);
        db.Database.EnsureCreated();
    }
}