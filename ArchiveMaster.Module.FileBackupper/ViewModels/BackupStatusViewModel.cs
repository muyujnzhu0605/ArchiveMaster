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

        public BackupStatusViewModel(FileBackupperConfig config, AppConfig appConfig )
        {
            Config = config;
            this.appConfig = appConfig;
            Tasks = new ObservableCollection<BackupTask>(config.Tasks);
        }

        [ObservableProperty]
        private ObservableCollection<BackupTask> tasks;

        [ObservableProperty]
        private BackupTask selectedTask;
        
        public FileBackupperConfig Config { get; }
        
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