using Microsoft.Extensions.Logging;

namespace ArchiveMaster.Models;

public class BackupLogEntity : EntityBase
{
    public int? SnapshotId { get; set; }
    public string Message { get; set; }
    public string Detail { get; set; }
    public LogLevel Type { get; set; }
    
    public DateTime Time { get; set; } = DateTime.Now;
}