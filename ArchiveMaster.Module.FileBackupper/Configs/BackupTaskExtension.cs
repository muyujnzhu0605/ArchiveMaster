using ArchiveMaster.Enums;
using ArchiveMaster.Utilities;

namespace ArchiveMaster.Configs;

public static class BackupTaskExtension
{
    public static void BeginBackup(this BackupTask task, bool isFullBackup)
    {
        task.Status = isFullBackup ? BackupTaskStatus.FullBackingUp : BackupTaskStatus.IncrementBackingUp;
    }

    public static void EndBackup(this BackupTask task, bool isFullBackup)
    {
        if (isFullBackup)
        {
            task.LastFullBackupTime = DateTime.Now;
        }

        task.LastBackupTime = DateTime.Now;

        task.Status = BackupTaskStatus.Ready;
    }

    public static async Task UpdateStatusAsync(this BackupTask task)
    {
        try
        {
            task.Check();
            await using DbService db = new DbService(task);
            var snapshots = await db.GetSnapshotsAsync();
            task.SnapshotCount = snapshots.Count;
            if (snapshots.Count > 0)
            {
                task.LastBackupTime = snapshots[^1].EndTime;
                for (int i = snapshots.Count - 1; i >= 0; i--)
                {
                    if (snapshots[i].IsFull)
                    {
                        task.LastFullBackupTime = snapshots[i].EndTime;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            task.Status = BackupTaskStatus.Error;
            task.Message = ex.Message;
        }
    }
    public static async Task UpdateStatusAsync(this IEnumerable<BackupTask> tasks)
    {
        foreach (var task in tasks)
        {
            await task.UpdateStatusAsync();
        }
    }
}