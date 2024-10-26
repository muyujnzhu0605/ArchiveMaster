using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using ArchiveMaster.Utilities;

namespace ArchiveMaster.Configs;

public static class BackupTaskExtension
{
    public static void BeginBackup(this BackupTask task, SnapshotType type)
    {
        task.Status = type switch
        {
            SnapshotType.Full => BackupTaskStatus.FullBackingUp,
            SnapshotType.VirtualFull => BackupTaskStatus.FullBackingUp,
            SnapshotType.Increment => BackupTaskStatus.IncrementBackingUp,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static void EndBackup(this BackupTask task, bool isFullBackup,bool succeed)
    {
        if (succeed)
        {
            if (isFullBackup)
            {
                task.LastFullBackupTime = DateTime.Now;
            }

            task.LastBackupTime = DateTime.Now;
        }

        task.Status = BackupTaskStatus.Ready;
    }

    public static bool IsEmpty(this BackupSnapshotEntity snapshot)
    {
        return snapshot.CreatedFileCount + snapshot.ModifiedFileCount + snapshot.DeletedFileCount == 0;
    }

    public static async Task UpdateStatusAsync(this BackupTask task)
    {
        try
        {
            task.Check();
            await using DbService db = new DbService(task);
            var snapshots = await db.GetSnapshotsAsync(includeEmptySnapshot: true);
            task.SnapshotCount = snapshots.Count;
            if (snapshots.Count > 0)
            {
                task.LastBackupTime = snapshots[^1].EndTime;
                for (int i = snapshots.Count - 1; i >= 0; i--)
                {
                    if (snapshots[i].Type is SnapshotType.Full or SnapshotType.VirtualFull)
                    {
                        task.LastFullBackupTime = snapshots[i].EndTime;
                    }
                }
            }

            if (task.Status == BackupTaskStatus.Error)
            {
                task.Status = BackupTaskStatus.Ready;
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