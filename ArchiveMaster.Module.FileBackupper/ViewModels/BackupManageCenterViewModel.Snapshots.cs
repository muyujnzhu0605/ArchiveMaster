using System.Collections.ObjectModel;
using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Models;
using ArchiveMaster.Services;
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

    [ObservableProperty]
    private int totalSnapshotCount;

    async partial void OnSelectedSnapshotChanged(BackupSnapshotEntity value)
    {
        if (value == null)
        {
            Logs = null;
            TreeFiles = null;
            FileHistory = null;
            SelectedFile = null;
            CreatedFiles = null;
            ModifiedFiles = null;
            DeletedFiles = null;
            return;
        }

        await TryDoAsync("加载快照详情", async () =>
        {
            LogSearchText = null;
            LogType = LogLevel.None;
            await LoadLogsAsync();
            await LoadFilesAsync();
            await LoadFileChangesAsync();
        });
    }


    [RelayCommand]
    private async Task LoadSnapshots()
    {
        if (await TryDoAsync("加载快照", async () =>
            {
                DbService db = new DbService(SelectedTask);
                Snapshots = new ObservableCollection<BackupSnapshotEntity>(await db.GetSnapshotsAsync());
            }) == false)
        {
            SelectedTask = null;
        }
    }
}