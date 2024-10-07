using ArchiveMaster.Configs;

namespace ArchiveMaster.Utilities;

public class FileBackupperUtility(BackupperTask task)
{
    public BackupperTask Task { get; } = task;

    public void FullBackup()
    {
        
    }
}