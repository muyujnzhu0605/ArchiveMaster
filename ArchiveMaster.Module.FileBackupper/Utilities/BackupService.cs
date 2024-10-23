using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ArchiveMaster.Utilities;

public class BackupService
{
    private readonly Dictionary<BackupTask, DateTime> lastBackupTimes = new Dictionary<BackupTask, DateTime>();

    public FileBackupperConfig Config { get; }

    public BackupService(FileBackupperConfig config)
    {
        Config = config;
    }

    public bool IsBackingUp { get; private set; }

    private CancellationTokenSource cts;
    private CancellationToken ct;

    public async void StartAutoBackup()
    {
        cts = new CancellationTokenSource();
        ct = cts.Token;
        Task.Factory.StartNew(async () =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await CheckAndBackupAllAsync();
                    await Task.Delay(60 * 1000, ct);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Log.Error(ex, "循环备份任务执行出错");
            }
        }, TaskCreationOptions.LongRunning);
    }

    public void Stop()
    {
        cts?.Cancel();
    }

    public Task StopAsync()
    {
        return cts?.CancelAsync() ?? Task.CompletedTask;
    }

    public async Task CheckAndBackupAllAsync()
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
                ct.ThrowIfCancellationRequested();

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
                var hasFullSnapshot = (await db.GetSnapshotsAsync(SnapshotType.Full, token: ct)).Count != 0
                                      || (await db.GetSnapshotsAsync(SnapshotType.VirtualFull, token: ct)).Count != 0;

                if (!hasFullSnapshot)
                {
                    await db.LogAsync(LogLevel.Information, $"未找到全量备份快照，即将开始全量备份，虚拟备份：{task.IsDefaultVirtualBackup}");
                    await utility.BackupAsync(
                        task.IsDefaultVirtualBackup ? SnapshotType.VirtualFull : SnapshotType.Full, ct);
                }
                else
                {
                    await utility.BackupAsync(SnapshotType.Increment, ct);
                }

                lastBackupTimes[task] = lastBackupTime;
            }
        }
        finally
        {
            IsBackingUp = false;
        }
    }
}