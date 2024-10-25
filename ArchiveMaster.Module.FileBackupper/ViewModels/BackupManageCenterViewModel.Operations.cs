using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.ViewModels.FileSystem;
using ArchiveMaster.Views;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;

namespace ArchiveMaster.ViewModels;

public partial class BackupManageCenterViewModel
{
    [RelayCommand]
    private void CancelMakingBackup()
    {
        MakeBackupCommand.Cancel();
    }
        
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task MakeBackupAsync(SnapshotType type, CancellationToken cancellationToken)
    {
        try
        {
            await backupService.MakeABackupAsync(SelectedTask, type, cancellationToken);
        }
        catch (Exception ex)
        {
            await this.ShowErrorAsync("备份失败", ex);
        }
    }
}