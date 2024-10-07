using ArchiveMaster.Enums;

namespace ArchiveMaster.Models;

public class BackupSnapshotEntity
{
    public BackupSnapshotType Type { get; set; }
    public DateTime Time { get; set; }
}