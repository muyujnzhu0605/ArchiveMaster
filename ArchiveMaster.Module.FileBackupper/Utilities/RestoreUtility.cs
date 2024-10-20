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

        await using var db = new DbService(task);
        await Task.Run(() =>
        {
            var fileRecords = db.GetLatestFiles(SnapShotId.Value);

            TreeDirInfo tree = TreeDirInfo.CreateEmptyTree();

            foreach (var record in fileRecords.Select(p => new BackupFile(p)))
            {
                tree.AddFile(record);
            }

            RootDir = tree;
        }, token);
    }
}