using ArchiveMaster.Configs;
using ArchiveMaster.Models;
using ArchiveMaster.ViewModels;
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
        await db.Database.EnsureCreatedAsync(token);
        var fileRecords =await db.Records
            .Where(p => p.SnapshotId == SnapShotId)
            .ToListAsync(token);
        
    }

    public async Task<IList<BackupSnapshotEntity>> GetSnapshotsAsync(CancellationToken token)
    {
        await using var db = new BackupperDbContext(task);
        await db.Database.EnsureCreatedAsync(token);
        var snapshots = await db.Snapshots.ToListAsync(token);
        return snapshots;
    }
}