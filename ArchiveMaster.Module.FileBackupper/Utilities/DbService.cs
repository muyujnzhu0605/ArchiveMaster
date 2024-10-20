using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using Microsoft.EntityFrameworkCore;

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

    public void AddFileRecord(FileRecordEntity record)
    {
        db.Records.Add(record);
    }

    public void AddPhysicalFile(PhysicalFileEntity file)
    {
        db.Files.Add(file);
    }

    public void AddSnapshot(BackupSnapshotEntity snapshot)
    {
        db.Snapshots.Add(snapshot);
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
        BackupSnapshotEntity snapshot = db.Snapshots
                                            .FirstOrDefault(p => p.Id == snapshotId)
                                        ?? throw new KeyNotFoundException(
                                            $"找不到ID为{snapshotId}的{nameof(BackupSnapshotEntity)}");
        return GetLatestFiles(snapshot);
    }

    public IEnumerable<FileRecordEntity> GetLatestFiles(BackupSnapshotEntity snapshot)
    {
        BackupSnapshotEntity fullSnapshot;
        List<BackupSnapshotEntity> incrementalSnapshots = new List<BackupSnapshotEntity>();
        if (snapshot.IsFull)
        {
            fullSnapshot = snapshot;
        }
        else
        {
            fullSnapshot = db.Snapshots
                               .Where(p => p.IsFull)
                               .Where(p => p.StartTime < snapshot.StartTime)
                               .OrderByDescending(p => p.StartTime)
                               .FirstOrDefault()
                           ?? throw new KeyNotFoundException(
                               $"找不到该{nameof(BackupSnapshotEntity)}对应的全量备份{nameof(BackupSnapshotEntity)}");
            incrementalSnapshots.AddRange(db.Snapshots
                .Where(p => !p.IsFull)
                .Where(p => p.StartTime > fullSnapshot.EndTime)
                .Where(p => p.EndTime < snapshot.StartTime)
                .OrderByDescending(p => p.StartTime)
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
        var query = db.Files
            .Where(p => p.Time == time)
            .Where(p => p.Length == length);
        if (sha1 != null)
        {
            query = query.Where(p => p.Hash == sha1);
        }

        return query.FirstOrDefault();
    }

    public Task<List<BackupSnapshotEntity>> GetSnapshotsAsync(CancellationToken token)
    {
        return db.Snapshots.ToListAsync(token);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }

    private void Initialize()
    {
        if (!initializedTasks.Contains(BackupTask))
        {
            db.Database.EnsureCreated();
            initializedTasks.Add(BackupTask);
        }
    }
}