using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

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
            newValue.PropertyChanged += SelectedBackupTaskPropertyChanged;
        }
    }

    private async void SelectedBackupTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender == SelectedTask)
        {
            if (e.PropertyName == nameof(BackupTask.Status)
                && (sender as BackupTask)?.Status == BackupTaskStatus.Ready)
            {
                await RefreshSnapshots();
            }
        }
    }
}