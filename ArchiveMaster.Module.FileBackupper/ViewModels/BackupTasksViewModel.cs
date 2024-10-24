using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using ArchiveMaster.Enums;
using FzLib.Avalonia.Messages;

namespace ArchiveMaster.ViewModels
{
    public partial class BackupTasksViewModel : ViewModelBase
    {
        [ObservableProperty]
        private BackupTask selectedTask;

        [ObservableProperty]
        private ObservableCollection<BackupTask> tasks;

        [ObservableProperty]
        private bool canSaveConfig = false;

        partial void OnSelectedTaskChanged(BackupTask oldValue, BackupTask newValue)
        {
            if (newValue != null)
            {
                newValue.PropertyChanged += SelectedBackupTaskPropertyChanged;
            }

            if (oldValue != null)
            {
                oldValue.PropertyChanged -= SelectedBackupTaskPropertyChanged;
            }
        }

        private void SelectedBackupTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyCanSaveConfig();
        }

        private void NotifyCanSaveConfig(bool canSave = true)
        {
            CanSaveConfig = canSave;
            SaveCommand.NotifyCanExecuteChanged();
        }

        public BackupTasksViewModel(FileBackupperConfig config, AppConfig appConfig)
        {
            Config = config;
            AppConfig = appConfig;
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
        }

        public AppConfig AppConfig { get; }
        public FileBackupperConfig Config { get; }

        public override async void OnEnter()
        {
            base.OnEnter();
            Tasks = new ObservableCollection<BackupTask>(Config.Tasks);
            await Tasks.UpdateStatusAsync();
            NotifyCanSaveConfig(false);
        }

        [RelayCommand]
        private void AddTask()
        {
            var task = new BackupTask();
            Tasks.Add(task);
            SelectedTask = task;
            NotifyCanSaveConfig();
        }

        [RelayCommand]
        private void DeleteSelectedTask()
        {
            Debug.Assert(SelectedTask != null);
            Tasks.Remove(SelectedTask);
            NotifyCanSaveConfig();
        }

        [RelayCommand(CanExecute = nameof(CanSaveConfig))]
        private void Save()
        {
            Config.Tasks = Tasks.Select(p => p.Clone() as BackupTask).ToList();
            AppConfig.Save();
            NotifyCanSaveConfig(false);
        }

        public override async Task OnExitAsync(CancelEventArgs args)
        {
            if (!CanSaveConfig)
            {
                return;
            }

            if ((await this.SendMessage(new CommonDialogMessage()
                {
                    Type = CommonDialogMessage.CommonDialogType.YesNo,
                    Title = "保存配置",
                    Message = "有未保存的配置，是否保存？"
                }).Task).Equals(true))
            {
                Save();
            }

            await base.OnExitAsync(args);
        }
    }
}