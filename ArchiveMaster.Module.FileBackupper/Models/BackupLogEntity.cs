using Microsoft.Extensions.Logging;

namespace ArchiveMaster.Models;

public class BackupLogEntity : EntityBase
{
    public int SnapshotId { get; set; }
    public BackupSnapshotEntity Snapshot { get; set; }
    public string Message { get; set; }
    public LogLevel Type { get; set; }
}