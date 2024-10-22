using ArchiveMaster.Models;

namespace ArchiveMaster.ViewModels;

public class BackupSnapshotWithFileCount
{
    public BackupSnapshotEntity Snapshot { get; set; }

    public int CreatedFileCount { get; set; }
    public int ModifiedFileCount { get; set; }
    public int DeletedFileCount { get; set; }
}