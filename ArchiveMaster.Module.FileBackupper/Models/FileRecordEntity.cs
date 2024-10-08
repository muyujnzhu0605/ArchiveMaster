using ArchiveMaster.Enums;

namespace ArchiveMaster.Models;

public class FileRecordEntity : EntityBase
{
    public BackupSnapshotEntity Snapshot { get; set; }

    /// <summary>
    /// 原始文件的相对路径
    /// </summary>
    public string RawFileRelativePath { get; set; }

    /// <summary>
    /// 对应的物理文件信息
    /// </summary>
    public PhysicalFileEntity PhysicalFile { get; set; }

    public int PhysicalFileId { get; set; }

    public string Message { get; set; }
    public ProcessStatus Status { get; set; }
    public int SnapshotId { get; set; }
    public FileRecordType Type { get; set; }
}