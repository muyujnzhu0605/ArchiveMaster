namespace ArchiveMaster.Models;

public class BackupSnapshotEntity : EntityBase
{
    /// <summary>
    /// 全量备份
    /// </summary>
    public bool IsFull { get; set; }

    /// <summary>
    /// 虚拟快照，当设置仅备份差异文件时，首次进行虚拟备份，仅将元数据写入数据库，不进行真正的文件复制
    /// </summary>
    public bool IsVirtual { get; set; }

    /// <summary>
    /// 备份开始的时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 备份开始的时间
    /// </summary>
    public DateTime EndTime { get; set; }
}