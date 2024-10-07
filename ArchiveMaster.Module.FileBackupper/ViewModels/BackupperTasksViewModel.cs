using ArchiveMaster.ViewModels;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using ArchiveMaster.Configs;
using ArchiveMaster.Messages;
using ArchiveMaster.Utilities;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels
{
    public partial class BackupperTasksViewModel : ViewModelBase
    {
        public BackupperTasksViewModel(FileBackupperConfig config)
        {
            Tasks = new ObservableCollection<BackupperTask>(config.Tasks);
#if DEBUG
            if (Tasks.Count == 0)
            {
                Tasks.Add(new BackupperTask()
                {
                    Name = "任务名",
                    SourceDir = @"C:\Users\autod\Desktop\备份源目录",
                    BackupDir = @"C:\Users\autod\Desktop\备份文件夹",
                    BlackList = "黑名单",
                    BlackListUseRegex = true
                });
            }
#endif
        }

        [ObservableProperty]
        private ObservableCollection<BackupperTask> tasks;

        [ObservableProperty]
        private BackupperTask selectedTask;

        [RelayCommand]
        private void AddTask()
        {
            var task = new BackupperTask();
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
            FileBackupperUtility utility = new FileBackupperUtility(SelectedTask);
            await utility.InitializeAsync();
            await utility.FullBackupAsync(false);
        }
    }
}