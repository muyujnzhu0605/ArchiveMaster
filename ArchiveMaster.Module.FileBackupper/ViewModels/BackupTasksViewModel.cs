using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArchiveMaster.ViewModels
{
    public partial class BackupTasksViewModel : ViewModelBase
    {
        private AppConfig appConfig;

        public BackupTasksViewModel(FileBackupperConfig config, AppConfig appConfig)
        {
            this.appConfig = appConfig;
#if DEBUG
            if (config.Tasks.Count == 0)
            {
                config.Tasks.Add(new BackupTask()
                {
                    Name = "任务名",
                    SourceDir = @"C:\Users\autod\Desktop\备份源目录",
                    BackupDir = @"C:\Users\autod\Desktop\备份文件夹",
                    BlackList = "黑名单",
                    BlackListUseRegex = true
                });
            }

            appConfig.Save();
#endif
            Tasks = new ObservableCollection<BackupTask>(config.Tasks);
        }

        [ObservableProperty]
        private ObservableCollection<BackupTask> tasks;

        [ObservableProperty]
        private BackupTask selectedTask;

        [RelayCommand]
        private void AddTask()
        {
            var task = new BackupTask();
            Tasks.Add(task);
            SelectedTask = task;
        }

        [RelayCommand]
        private void DeleteSelectedTask()
        {
            Debug.Assert(SelectedTask != null);
            Tasks.Remove(SelectedTask);
        }

        [RelayCommand]
        private async Task TestFullBackupAsync()
        {
            BackupUtility utility = new BackupUtility(SelectedTask);
            await utility.FullBackupAsync(false);
        }
    }
}