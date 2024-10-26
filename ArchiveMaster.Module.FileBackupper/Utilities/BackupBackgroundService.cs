using System.Diagnostics;
using ArchiveMaster.Configs;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;

namespace ArchiveMaster.Utilities;

public class BackupBackgroundService(BackupService backupService) : IHostedService
{
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    
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