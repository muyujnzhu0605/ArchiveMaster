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

        [ObservableProperty]
        private string selectedIncludingFile;

        [ObservableProperty]
        private string selectedIncludingFolder;

        [RelayCommand]
        private async Task BrowseIncludingFileAsync()
        {
            Debug.Assert(SelectedTask != null);
            var storageProvider = this.SendMessage(new GetStorageProviderMessage()).StorageProvider;
            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                AllowMultiple = true,
            });
            foreach (var file in files)
            {
                var path = file.TryGetLocalPath();
                if (!SelectedTask.IncludingFiles.Contains(path))
                {
                    SelectedTask.IncludingFiles.Add(path);
                }
            }
        }

        [RelayCommand]
        private async Task InputIncludingFileAsync()
        {
            Debug.Assert(SelectedTask != null);
            if (await this.SendMessage(new InputDialogMessage()
                {
                    Type = InputDialogMessage.InputDialogType.Text,
                    Title = "输入目录",
                    Message = "请输入欲加入的目录地址"
                }).Task is string path)
            {
                if (File.Exists(path))
                {
                    if (!SelectedTask.IncludingFiles.Contains(path))
                    {
                        SelectedTask.IncludingFiles.Add(path);
                    }
                    else
                    {
                        await this.ShowErrorAsync("加入失败", "列表中已存在该文件");
                    }
                }
                else
                {
                    await this.ShowErrorAsync("加入失败", "文件不存在");
                }
            }
        }

        [RelayCommand]
        private void RemoveIncludingFile()
        {
            Debug.Assert(SelectedTask != null);
            Debug.Assert(SelectedIncludingFile != null);
            SelectedTask.IncludingFiles.Remove(SelectedIncludingFile);
        }
        
        [RelayCommand]
        private async Task BrowseIncludingFolderAsync()
        {
            Debug.Assert(SelectedTask != null);
            var storageProvider = this.SendMessage(new GetStorageProviderMessage()).StorageProvider;
            var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                AllowMultiple = true,
            });
            foreach (var folder in folders)
            {
                var path = folder.TryGetLocalPath();
                if (!SelectedTask.IncludingFolders.Contains(path))
                {
                    SelectedTask.IncludingFolders.Add(path);
                }
            }
        }

        [RelayCommand]
        private async Task InputIncludingFolderAsync()
        {
            Debug.Assert(SelectedTask != null);
            if (await this.SendMessage(new InputDialogMessage()
                {
                    Type = InputDialogMessage.InputDialogType.Text,
                    Title = "输入目录",
                    Message = "请输入欲加入的目录地址"
                }).Task is string path)
            {
                if (Directory.Exists(path))
                {
                    if (!SelectedTask.IncludingFolders.Contains(path))
                    {
                        SelectedTask.IncludingFolders.Add(path);
                    }
                    else
                    {
                        await this.ShowErrorAsync("加入失败", "列表中已存在该文件");
                    }
                }
                else
                {
                    await this.ShowErrorAsync("加入失败", "文件不存在");
                }
            }
        }

        [RelayCommand]
        private void RemoveIncludingFolder()
        {
            Debug.Assert(SelectedTask != null);
            Debug.Assert(SelectedIncludingFolder != null);
            SelectedTask.IncludingFolders.Remove(SelectedIncludingFolder);
        }

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
    }
}