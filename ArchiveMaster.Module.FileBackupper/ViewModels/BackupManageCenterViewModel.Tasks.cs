using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArchiveMaster.ViewModels;

public partial class BackupManageCenterViewModel
{
    [ObservableProperty]
    private BackupTask selectedTask;


    [ObservableProperty]
    private ObservableCollection<BackupTask> tasks;

    async partial void OnSelectedTaskChanged(BackupTask oldValue, BackupTask newValue)
    {
        Snapshots = null;
        Logs = null;
        TreeFiles = null;

        if (oldValue != null)
        {
            oldValue.PropertyChanged -= SelectedBackupTaskPropertyChanged;
        }

        if (newValue != null)
        {
            await RefreshSnapshots();
            SelectedBackupTaskPropertyChanged(SelectedTask, new PropertyChangedEventArgs(nameof(BackupTask.Status)));
            newValue.PropertyChanged += SelectedBackupTaskPropertyChanged;
        }
    }

    private async void SelectedBackupTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (SelectedTask == null || sender != SelectedTask)
        {
            return;
        }

        if (e.PropertyName == nameof(BackupTask.Status))
        {
            switch ((sender as BackupTask)?.Status)
            {
                case BackupTaskStatus.Ready:
                    CanMakeBackup = true;
                    IsTaskOperationEnable = true;
                    await SelectedTask.UpdateStatusAsync();
                    await RefreshSnapshots();
                    break;
                case BackupTaskStatus.FullBackingUp:
                case BackupTaskStatus.IncrementBackingUp:
                    CanMakeBackup = false;
                    IsTaskOperationEnable = true;
                    break;
                default:
                    IsTaskOperationEnable = true;
                    return;
            }
        }
    }

    [RelayCommand]
    private void CancelMakingBackup()
    {
        MakeBackupCommand.Cancel();
    }

    [ObservableProperty]
    private bool canMakeBackup = true;

    [ObservableProperty]
    private bool isTaskOperationEnable;

    [RelayCommand(IncludeCancelCommand = true, CanExecute = nameof(CanMakeBackup))]
    private async Task MakeBackupAsync(SnapshotType type, CancellationToken cancellationToken)
    {
        try
        {
            await backupService.MakeABackupAsync(SelectedTask, type, cancellationToken);
        }
        catch (Exception ex)
        {
            await this.ShowErrorAsync("备份失败", ex);
        }
    }
}