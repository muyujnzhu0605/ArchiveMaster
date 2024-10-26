using System.ComponentModel.DataAnnotations;
using ArchiveMaster.Enums;

namespace ArchiveMaster.Models;

public class BackupFileEntity : EntityBase
{
    /// <summary>
    /// 对应的备份后的物理文件名，使用GUID的32位文件名
    /// </summary>
    [MaxLength(32)]
    public string BackupFileName { get; set; }

    /// <summary>
    /// SHA1的哈希值，长度为40
    /// </summary>
    [MaxLength(40)]
    public string Hash { get; set; }

    /// <summary>
    /// 文件字节数
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// 原始文件的相对路径
    /// </summary>
    public string RawFileRelativePath { get; set; }

    public BackupSnapshotEntity Snapshot { get; set; }
    public int SnapshotId { get; set; }

    public ProcessStatus Status { get; set; }

    /// <summary>
    /// 文件修改时间
    /// </summary>
    public DateTime Time { get; set; }
    public FileRecordType Type { get; set; }
}