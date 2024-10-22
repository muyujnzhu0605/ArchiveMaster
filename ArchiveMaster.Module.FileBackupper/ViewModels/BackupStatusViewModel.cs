using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ArchiveMaster.Converters;
using ArchiveMaster.Models;
using FzLib.Avalonia.Messages;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.ViewModels
{
    public partial class BackupStatusViewModel : ViewModelBase
    {
        private AppConfig appConfig;

        [ObservableProperty]
        private BackupTask selectedTask;

        [ObservableProperty]
        private int selectedTabIndex;

        async partial void OnSelectedTaskChanged(BackupTask value)
        {
            try
            {
                if (value == null)
                {
                    Snapshots = null;
                }
                else
                {
                    DbService db = new DbService(value);

                    CanSelectTasks = false;
                    Snapshots = new ObservableCollection<BackupSnapshotWithFileCount>(
                        await db.GetSnapshotsWithFilesAsync());
                }
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

        [ObservableProperty]
        private ObservableCollection<BackupTask> tasks;

        [ObservableProperty]
        private ObservableCollection<BackupLogEntity> logs;

        [ObservableProperty]
        private ObservableCollection<BackupSnapshotWithFileCount> snapshots;

        [ObservableProperty]
        private BackupSnapshotWithFileCount selectedSnapshot;

        [ObservableProperty]
        private bool canSelectTasks = true;

        public BackupStatusViewModel(FileBackupperConfig config, AppConfig appConfig)
        {
            Config = config;
            this.appConfig = appConfig;
        }

        public FileBackupperConfig Config { get; }

        public override async void OnEnter()
        {
            Tasks = new ObservableCollection<BackupTask>(Config.Tasks);
            await Tasks.UpdateStatusAsync();
        }

        [RelayCommand]
        private async Task TestFullBackupAsync()
        {
            BackupUtility utility = new BackupUtility(SelectedTask);
            await utility.FullBackupAsync(false);
        }

        [RelayCommand]
        private async Task TestIncrementalBackupAsync()
        {
            BackupUtility utility = new BackupUtility(SelectedTask);
            await utility.IncrementalBackupAsync();
        }

        [RelayCommand]
        private async Task JumpToLogsBySnapshotAsync(BackupSnapshotWithFileCount snapshot)
        {
            SelectedTabIndex = 1;
            await using var db = new DbService(SelectedTask);
            Logs = new ObservableCollection<BackupLogEntity>(await db.GetLogsAsync(snapshot.Snapshot.Id));
        }

        [RelayCommand]
        private Task ShowDetailAsync(BackupLogEntity log)
        {
            return this.SendMessage(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Ok,
                Message = log.Message,
                Title = LogLevelConverter.GetDescription(log.Type),
                Detail = log.Detail
            }).Task;
        }
    }
}