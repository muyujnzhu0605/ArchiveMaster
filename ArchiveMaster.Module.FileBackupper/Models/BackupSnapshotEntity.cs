using ArchiveMaster.Enums;

namespace ArchiveMaster.Models;

public class BackupSnapshotEntity : EntityBase
{
    /// <summary>
    /// 快照类型
    /// </summary>
    public SnapshotType Type { get; set; }

    /// <summary>
    /// 备份开始的时间
    /// </summary>
    public DateTime BeginTime { get; set; }

    /// <summary>
    /// 备份开始的时间
    /// </summary>
    public DateTime EndTime { get; set; }
}