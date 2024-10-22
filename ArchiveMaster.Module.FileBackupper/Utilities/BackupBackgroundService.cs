using System.Diagnostics;
using ArchiveMaster.Configs;
using ArchiveMaster.Models;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ArchiveMaster.Utilities;

public class BackupBackgroundService : IHostedService
{
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private readonly CancellationToken cancellationToken;
    public FileBackupperConfig Config { get; }

    public bool IsBackingUp { get; private set; }

    private Timer timer;

    private readonly Dictionary<BackupTask, DateTime> lastBackupTimes = new Dictionary<BackupTask, DateTime>();

    public BackupBackgroundService(FileBackupperConfig config)
    {
        Config = config;
        cancellationToken = cancellationTokenSource.Token;
    }

    private async Task CheckTasksAndExecuteAsync()
    {
        if (!Config.EnableBackgroundBackup)
        {
            return;
        }

        if (IsBackingUp)
        {
            throw new InvalidOperationException("正在备份，无法进行新一轮的检查和执行");
        }

        IsBackingUp = true;
        try
        {
            foreach (var task in Config.Tasks.Where(p => p.ByTimeInterval).ToList())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (!Config.Tasks.Contains(task))
                {
                    continue; //防止在长时间备份时，任务被删除
                }

                var interval = task.TimeInterval;
                if (interval.TotalMinutes < 1)
                {
                    //防止间隔被设置得很短
                    interval = TimeSpan.FromMinutes(1);
                }

                if (lastBackupTimes.TryGetValue(task, out var lastBackupTime))
                {
                    if (lastBackupTime + interval > DateTime.Now) //下一次备份时间还没到
                    {
                        continue;
                    }
                }

                //开始备份
                await using var db = new DbService(task);
                await db.LogAsync(LogLevel.Information, $"根据间隔时间备份规则，已到应备份时间");
                BackupUtility utility = new BackupUtility(task);
                List<BackupSnapshotEntity> snapshots = await db.GetSnapshotsAsync(true, token: cancellationToken);

                if (snapshots.Count == 0)
                {
                    await db.LogAsync(LogLevel.Information, $"未找到全量备份快照，即将开始全量备份，虚拟备份：{task.IsDefaultVirtualBackup}");
                    await utility.FullBackupAsync(task.IsDefaultVirtualBackup, cancellationToken);
                }
                else
                {
                    await utility.IncrementalBackupAsync(cancellationToken);
                }

                lastBackupTimes[task] = lastBackupTime;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "备份任务执行出错");
        }
        finally
        {
            IsBackingUp = false;
        }
    }

    public Task StartAsync(CancellationToken _)
    {
        if (Design.IsDesignMode)
        {
            return Task.CompletedTask;
        }


        return Task.Factory.StartNew(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await CheckTasksAndExecuteAsync();
                await Task.Delay(60 * 1000, cancellationToken);
            }
        }, TaskCreationOptions.LongRunning);
    }

    public async Task StopAsync(CancellationToken _)
    {
        await cancellationTokenSource.CancelAsync();
    }
}