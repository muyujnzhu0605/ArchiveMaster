using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArchiveMaster.ViewModels
{
    public partial class BackupStatusViewModel : ViewModelBase
    {
        private AppConfig appConfig;

        [ObservableProperty]
        private BackupTask selectedTask;

        [ObservableProperty]
        private ObservableCollection<BackupTask> tasks;

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
    }
}