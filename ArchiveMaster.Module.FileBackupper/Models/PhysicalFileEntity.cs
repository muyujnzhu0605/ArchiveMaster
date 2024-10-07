using System.ComponentModel.DataAnnotations;

namespace ArchiveMaster.Models;

public class PhysicalFileEntity : EntityBase
{
    /// <summary>
    /// 对应的备份后的物理文件名，使用GUID的32位文件名
    /// </summary>
    [MaxLength(32)]
    public string FileName { get; set; }
    
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
    /// 文件修改时间
    /// </summary>
    public DateTime Time { get; set; }
    
    /// <summary>
    /// 被引用的数量
    /// </summary>
    public int RefCount { get; set; }
    
    /// <summary>
    /// 对应的全量备份的ID。每次全量备份时，将重新复制文件，因此早于当次全量备份的文件将不再起作用。
    /// </summary>
    public int FullSnapshotId { get; set; }
    
    /// <summary>
    /// 对应的全量备份。每次全量备份时，将重新复制文件，因此早于当次全量备份的文件将不再起作用。
    /// </summary>
    public BackupSnapshotEntity FullSnapshot { get; set; }
}