using System.Collections.ObjectModel;
using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using ArchiveMaster.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;
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
            SelectedTabIndex = 0;
            LogSearchText = null;
            LogType = LogLevel.None;
            LogTimeFrom = value.BeginTime.AddHours(-1);
            LogTimeTo = value.EndTime.AddHours(1);
            await LoadLogsAsync();
            await LoadFilesAsync();
            await LoadFileChangesAsync();
        });
    }


    [RelayCommand]
    private async Task LoadSnapshotsAsync()
    {
        try
        {
            DbService db = new DbService(SelectedTask);
            Snapshots = new ObservableCollection<BackupSnapshotEntity>(await db.GetSnapshotsAsync());
        }
        catch (Exception ex)
        {
            SelectedTask = null;
            throw;
        }
    }

    [RelayCommand]
    private async Task DeleteSnapshotAsync(BackupSnapshotEntity snapshot)
    {
        string message = null;
        int index = Snapshots.IndexOf(snapshot);
        if (index == Snapshots.Count - 1) //最后一个，可以直接删
        {
            message = "是否删除此快照？";
        }
        else
        {
            if (Snapshots[index + 1].Type is SnapshotType.Increment) //后面跟着增量备份，会把后面的也一起删了
            {
                message = "删除此快照，将同步删除后续的增量快照，是否删除此快照？";
            }
            else //后面跟着全量备份，无影响
            {
                message = "是否删除此快照？";
            }
        }

        bool confirm = true.Equals(await this.SendMessage(new CommonDialogMessage()
        {
            Type = CommonDialogMessage.CommonDialogType.YesNo,
            Title = "删除快照",
            Message = message
        }).Task);

        if (confirm)
        {
            await TryDoAsync("删除快照", async () =>
            {
                ThrowIfIsBackingUp();
                await using var db = new DbService(SelectedTask);
                await db.DeleteSnapshotAsync(snapshot);
                await LoadSnapshotsAsync();
            });
        }
    }
}