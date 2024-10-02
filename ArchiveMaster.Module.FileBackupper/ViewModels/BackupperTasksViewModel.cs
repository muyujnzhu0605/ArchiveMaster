using ArchiveMaster.ViewModels;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels
{
    public partial class BackupperTasksViewModel : ViewModelBase
    {
        public BackupperTasksViewModel()
        {
#if DEBUG
            if (Tasks.Count == 0)
            {
                Tasks.Add(new BackupperTask()
                {
                    Name = "任务名",
                    IncludingFolders =
                    {
                        @"C:\Users\autod\Desktop\备份测试1",
                        @"C:\Users\autod\Desktop\备份测试2"
                    },
                    IncludingFiles =
                    {
                        @"C:\Users\autod\Desktop\备份文件1",
                        @"C:\Users\autod\Desktop\备份文件2"
                    },
                    BackupDirs = @"C:\Users\autod\Desktop\备份文件夹"
                });
            }
#endif
        }

        [ObservableProperty]
        private ObservableCollection<BackupperTask> tasks
            = new ObservableCollection<BackupperTask>(
                Services.Provider.GetRequiredService<FileBackupperConfig>().Tasks);

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
            if (SelectedTask != null)
            {
                Tasks.Remove(SelectedTask);
            }
        }
    }
}