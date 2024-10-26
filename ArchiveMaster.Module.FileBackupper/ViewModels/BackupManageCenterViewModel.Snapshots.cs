using System.Collections.ObjectModel;
using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Models;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.ViewModels;

public partial class BackupManageCenterViewModel
{
    [ObservableProperty]
    private BackupSnapshotEntity selectedSnapshot;

    [ObservableProperty]
    private ObservableCollection<BackupSnapshotEntity> snapshots;

    async partial void OnSelectedSnapshotChanged(BackupSnapshotEntity value)
    {
        if (value == null)
        {
            Logs = null;
            TreeFiles = null;
            FileHistory = null;
            SelectedFile = null;
            return;
        }

        await TryDoAsync("加载快照文件和日志", async () =>
        {
            LogSearchText = null;
            LogType = LogLevel.None;
            await LoadLogsAsync();
            await LoadFilesAsync();
        });
    }


    [RelayCommand]
    private Task RefreshSnapshots()
    {
        return TryDoAsync("加载快照", async () =>
        {
            DbService db = new DbService(SelectedTask);
            Snapshots = new ObservableCollection<BackupSnapshotEntity>(
                await db.GetSnapshotsAsync());
        });
    }
}