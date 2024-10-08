using ArchiveMaster.Configs;
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
        await using var db = new BackupperDbContext(task);
        await Task.Run(() =>
        {
            db.Database.EnsureCreated();

            BackupSnapshotEntity snapshot = db.Snapshots
                                                .FirstOrDefault(p => p.Id == SnapShotId)
                                            ?? throw new KeyNotFoundException(
                                                $"找不到ID为{SnapShotId}的{nameof(BackupSnapshotEntity)}");
            BackupSnapshotEntity fullSnapshot;
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
            }

            var fileRecords = db.Records
                .Where(p => p.SnapshotId == fullSnapshot.Id)
                .Include(p => p.PhysicalFile)
                .AsEnumerable()
                .Select(p => new BackupFile(p))
                .ToList();

            TreeDirInfo tree = TreeDirInfo.CreateEmptyTree();

            foreach (var record in fileRecords)
            {
                tree.AddFile(record);
            }

            RootDir = tree;
        });
    }

    public Task<List<BackupSnapshotEntity>> GetSnapshotsAsync(CancellationToken token)
    {
        return Task.Run(() =>
        {
            using var db = new BackupperDbContext(task);
            var snapshots = db.Snapshots.ToList();
            return snapshots;
        });
    }
}