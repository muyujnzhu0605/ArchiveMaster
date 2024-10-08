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
}