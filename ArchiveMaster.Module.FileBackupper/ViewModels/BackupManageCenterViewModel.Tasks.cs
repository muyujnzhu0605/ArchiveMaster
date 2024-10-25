using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public partial class BackupManageCenterViewModel
{

    [ObservableProperty]
    private bool canSelectTasks = true;

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
            try
            {
                CanSelectTasks = false;
                await UpdateSnapshots(newValue);
                newValue.PropertyChanged += SelectedBackupTaskPropertyChanged;
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync("加载快照失败", ex);
            }
            finally
            {
                CanSelectTasks = true;
            }
        }
    }

    private async void SelectedBackupTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BackupTask.Status) 
            && (sender as BackupTask)?.Status == BackupTaskStatus.Ready)
        {
            await UpdateSnapshots((BackupTask)sender);
        }
    }
}