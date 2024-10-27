using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using ArchiveMaster.Basic;
using ArchiveMaster.Converters;
using ArchiveMaster.Enums;
using ArchiveMaster.Messages;
using ArchiveMaster.Models;
using ArchiveMaster.ViewModels.FileSystem;
using ArchiveMaster.Views;
using Avalonia.Platform.Storage;
using FzLib.Avalonia.Messages;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.ViewModels
{
    public partial class BackupManageCenterViewModel : ViewModelBase
    {
        private readonly BackupService backupService;

        private AppConfig appConfig;

        [ObservableProperty]
        private int selectedTabIndex;

        public BackupManageCenterViewModel(FileBackupperConfig config, AppConfig appConfig, BackupService backupService)
        {
            Config = config;
            this.appConfig = appConfig;
            this.backupService = backupService;
        }

        public FileBackupperConfig Config { get; }

        public override async void OnEnter()
        {
            base.OnEnter();
            await LoadTasksAsync();
        }

        private async Task<bool> TryDoAsync(string workName, Func<Task> task)
        {
            this.SendMessage(new LoadingMessage(true));
            await Task.Delay(100);
            try
            {
                await task();
                this.SendMessage(new LoadingMessage(false));
                return true;
            }
            catch (Exception ex)
            {
                this.SendMessage(new LoadingMessage(false));
                await this.ShowErrorAsync($"{workName}失败", ex);
                return false;
            }
        }
    }
}