using System.Diagnostics;
using ArchiveMaster.Configs;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;

namespace ArchiveMaster.Services;

public class BackupBackgroundService(BackupService backupService, FileBackupperConfig config) : IBackgroundService
{
    public FileBackupperConfig Config { get; } = config;

    public bool IsEnabled => Config.EnableBackgroundBackup;

    public Task StartAsync(CancellationToken _)
    {
        if (Design.IsDesignMode)
        {
            return Task.CompletedTask;
        }

        backupService.StartAutoBackup();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken _)
    {
        await backupService.StopAutoBackupAsync();
    }
}