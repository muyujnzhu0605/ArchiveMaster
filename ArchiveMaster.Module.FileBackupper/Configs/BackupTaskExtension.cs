using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using ArchiveMaster.Services;

namespace ArchiveMaster.Configs;

public static class BackupTaskExtension
{
    public static void BeginBackup(this BackupTask task, SnapshotType type)
    {
        task.Check();
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
        task.Message = "";
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
            task.SnapshotCount = await db.GetSnapshotCountAsync(includeEmptySnapshot: true);
            task.ValidSnapshotCount = await db.GetSnapshotCountAsync();
            task.LastBackupTime = (await db.GetLastSnapshotAsync())?.BeginTime ?? default;
            var dt1 = (await db.GetLastSnapshotAsync(SnapshotType.Full))?.BeginTime ?? default;
            var dt2 = (await db.GetLastSnapshotAsync(SnapshotType.VirtualFull))?.BeginTime ?? default;
            task.LastFullBackupTime = dt1 > dt2 ? dt1 : dt2;

            if (task.Status == BackupTaskStatus.Error)
            {
                task.Status = BackupTaskStatus.Ready;
                task.Message = "";
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