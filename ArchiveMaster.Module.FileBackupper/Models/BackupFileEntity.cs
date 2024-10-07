using ArchiveMaster.Enums;

namespace ArchiveMaster.Models;

public class BackupFileEntity : EntityBase
{
    public BackupSnapshotEntity Snapshot { get; set; }

    /// <summary>
    /// 备份文件的名称，为随机GUID
    /// </summary>
    public string BackupFileName { get; set; }

    /// <summary>
    /// 原始文件的相对路径
    /// </summary>
    public string RawFileRelativePath { get; set; }

    public string Message { get; set; }
    public ProcessStatus Status { get; set; }
    public int SnapshotId { get; set; }
}