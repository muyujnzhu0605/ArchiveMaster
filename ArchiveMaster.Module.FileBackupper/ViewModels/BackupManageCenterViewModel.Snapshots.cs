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
    
    [RelayCommand]
    private async Task JumpToLogsBySnapshotAsync(BackupSnapshotWithFileCount snapshot)
    {
        SelectedTabIndex = 2;
        await using var db = new DbService(SelectedTask);
        Logs = new ObservableCollection<BackupLogEntity>(await db.GetLogsAsync(snapshot.Snapshot.Id));
    }

    [RelayCommand]
    private async Task JumpToRestoreBySnapshotAsync(BackupSnapshotWithFileCount snapshot)
    {
        SelectedTabIndex = 1;
        var utility = new RestoreUtility(SelectedTask);
        var tree = await utility.GetSnapshotFileTreeAsync(snapshot.Snapshot.Id);
        tree.Reorder();
        tree.Name = $"快照{snapshot.Snapshot.BeginTime}";

        TreeFiles = new BulkObservableCollection<SimpleFileInfo>();
        TreeFiles.Add(tree);
    }
    
    private async Task UpdateSnapshots(BackupTask newValue)
    {
        DbService db = new DbService(newValue);
        Snapshots = new ObservableCollection<BackupSnapshotWithFileCount>(
            await db.GetSnapshotsWithFilesAsync());
    }
}