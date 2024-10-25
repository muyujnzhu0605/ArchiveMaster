using System.Collections.Concurrent;
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
    private readonly BackupperDbContext logDb;

    public DbService(BackupTask backupTask)
    {
        BackupTask = backupTask;
        db = new BackupperDbContext(backupTask);
        logDb = new BackupperDbContext(backupTask);
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
        if (!logs.IsEmpty)
        {
            Debug.WriteLine("保存日志");
            logDb.Logs.AddRange(logs);
            logs.Clear();
            logDb.SaveChanges();
        }

        db?.Dispose();
        logDb?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (!logs.IsEmpty)
        {
            Debug.WriteLine("保存日志");
            logDb.Logs.AddRange(logs);
            logs.Clear();
            await logDb.SaveChangesAsync();
        }

        if (db != null)
        {
            await db.DisposeAsync();
        }

        if (logDb != null)
        {
            await logDb.DisposeAsync();
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
        if (snapshot.Type is SnapshotType.Full or SnapshotType.VirtualFull)
        {
            fullSnapshot = snapshot;
        }
        else
        {
            fullSnapshot = GetValidSnapshots()
                               .Where(p => p.Type == SnapshotType.Full || p.Type == SnapshotType.VirtualFull)
                               .Where(p => p.BeginTime < snapshot.BeginTime)
                               .OrderByDescending(p => p.BeginTime)
                               .FirstOrDefault()
                           ?? throw new KeyNotFoundException(
                               $"找不到该{nameof(BackupSnapshotEntity)}对应的全量备份{nameof(BackupSnapshotEntity)}");
            incrementalSnapshots.AddRange(GetValidSnapshots()
                .Where(p => p.Type == SnapshotType.Increment)
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

    public async Task<List<BackupSnapshotWithFileCount>> GetSnapshotsWithFilesAsync(SnapshotType? type = null,
        CancellationToken token = default)
    {
        await InitializeAsync(token);
        var query = GetSnapshotsQuery(type).Select(s => new BackupSnapshotWithFileCount()
        {
            Snapshot = s,
            CreatedFileCount = db.Records.Count(p => p.Type == FileRecordType.Created && p.SnapshotId == s.Id),
            DeletedFileCount = db.Records.Count(p => p.Type == FileRecordType.Deleted && p.SnapshotId == s.Id),
            ModifiedFileCount = db.Records.Count(p => p.Type == FileRecordType.Modified && p.SnapshotId == s.Id),
        });
        return await query.ToListAsync(token).ConfigureAwait(false);
    }

    private IQueryable<BackupSnapshotEntity> GetSnapshotsQuery(SnapshotType? type = null)
    {
        IQueryable<BackupSnapshotEntity> query = GetValidSnapshots();
        if (type.HasValue)
        {
            query = query.Where(p => p.Type == type);
        }

        query = query.OrderBy(p => p.BeginTime);
        return query;
    }


    public async Task<List<BackupSnapshotEntity>> GetSnapshotsAsync(SnapshotType? type = null,
        CancellationToken token = default)
    {
        await InitializeAsync(token);
        return await GetSnapshotsQuery(type).ToListAsync(token).ConfigureAwait(false);
    }

    private readonly ConcurrentBag<BackupLogEntity> logs = new ConcurrentBag<BackupLogEntity>();

    public async ValueTask LogAsync(LogLevel logLevel, string message, BackupSnapshotEntity snapshot = null,
        string detail = null, bool forceSave = false)
    {
        await InitializeAsync(default, logDb);
        BackupLogEntity log = new BackupLogEntity()
        {
            Message = message,
            Type = logLevel,
            SnapshotId = snapshot?.Id,
            Detail = detail
        };
        //Debug.WriteLine($"{DateTime.Now}\t\t{message}");
        logs.Add(log);
        if (forceSave || logs.Count >= 1000)
        {
            Debug.WriteLine("保存日志");
            logDb.Logs.AddRange(logs);
            logs.Clear();
            await logDb.SaveChangesAsync();
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<BackupLogEntity>> GetLogsAsync(int? snapshotId = null, LogLevel? type = null,string searchText=null)
    {
        await InitializeAsync();
        IQueryable<BackupLogEntity> query = db.Logs;
        if (snapshotId.HasValue)
        {
            query = query.Where(p => p.SnapshotId == snapshotId.Value);
        }

        if (type.HasValue && type.Value is not LogLevel.None)
        {
            query = query.Where(p => p.Type == type.Value);
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(p => p.Message.Contains(searchText));
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