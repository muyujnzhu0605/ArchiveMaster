using System.Collections.ObjectModel;
using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Models;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArchiveMaster.ViewModels;

public partial class BackupManageCenterViewModel
{
    [ObservableProperty]
    private BackupSnapshotWithFileCount selectedSnapshot;

    [ObservableProperty]
    private ObservableCollection<BackupSnapshotWithFileCount> snapshots;

    async partial void OnSelectedSnapshotChanged(BackupSnapshotWithFileCount value)
    {
        if (value == null)
        {
            Logs = null;
            TreeFiles = null;
            return;
        }
        await TryDoAsync("加载快照文件和日志", async () =>
        {
            await using var db = new DbService(SelectedTask);
            Logs = new ObservableCollection<BackupLogEntity>(await db.GetLogsAsync(value.Snapshot.Id));

            var utility = new RestoreUtility(SelectedTask);
            var tree = await utility.GetSnapshotFileTreeAsync(value.Snapshot.Id);
            tree.Reorder();
            tree.Name = $"快照{value.Snapshot.BeginTime}";

            TreeFiles = new BulkObservableCollection<SimpleFileInfo>();
            TreeFiles.Add(tree);
        });
    }


    [RelayCommand]
    private Task RefreshSnapshots()
    {
        return TryDoAsync("加载快照", async () =>
        {
            DbService db = new DbService(SelectedTask);
            Snapshots = new ObservableCollection<BackupSnapshotWithFileCount>(
                await db.GetSnapshotsWithFilesAsync());
        });
    }
}