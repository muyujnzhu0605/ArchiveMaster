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

    /// <summary>
    /// 新增的文件数量
    /// </summary>
    public int CreatedFileCount { get; set; }
    
    /// <summary>
    /// 修改的文件数量
    /// </summary>
    public int ModifiedFileCount { get; set; }
    
    /// <summary>
    /// 删除的文件数量
    /// </summary>
    public int DeletedFileCount { get; set; }
}