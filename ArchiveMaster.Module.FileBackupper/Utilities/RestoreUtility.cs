using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using ArchiveMaster.ViewModels;
using ArchiveMaster.ViewModels.FileSystem;
using Microsoft.EntityFrameworkCore;

namespace ArchiveMaster.Utilities;

public class RestoreUtility(BackupTask task, AppConfig appConfig) : TwoStepUtilityBase<BackupTask>(task, appConfig)
{
    public TreeDirInfo RootDir { get; private set; }
    public int? SnapShotId { get; set; }

    public override Task ExecuteAsync(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public override async Task InitializeAsync(CancellationToken token = default)
    {
        if (!SnapShotId.HasValue)
        {
            throw new InvalidOperationException("还未设置快照ID");
        }

        await using var db = new BackupperDbContext(task);
        await Task.Run(() =>
        {
            db.Database.EnsureCreated();

            var fileRecords = GetLatestFiles(db, SnapShotId.Value);

            TreeDirInfo tree = TreeDirInfo.CreateEmptyTree();

            foreach (var record in fileRecords.Select(p => new BackupFile(p)))
            {
                tree.AddFile(record);
            }

            RootDir = tree;
        });
    }

    public static IEnumerable<FileRecordEntity> GetLatestFiles(BackupperDbContext db, int snapshotId)
    {
        BackupSnapshotEntity snapshot = db.Snapshots
                                            .FirstOrDefault(p => p.Id == snapshotId)
                                        ?? throw new KeyNotFoundException(
                                            $"找不到ID为{snapshotId}的{nameof(BackupSnapshotEntity)}");
        return GetLatestFiles(db, snapshotId);
    }

    public static IEnumerable<FileRecordEntity> GetLatestFiles(BackupperDbContext db, BackupSnapshotEntity snapshot)
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

    public Task<List<BackupSnapshotEntity>> GetSnapshotsAsync(CancellationToken token)
    {
        return Task.Run(() =>
        {
            using var db = new BackupperDbContext(task);
            var snapshots = db.Snapshots.ToListAsync(token);
            return snapshots;
        }, token);
    }
}