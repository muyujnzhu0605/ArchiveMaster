namespace ArchiveMaster.Models;

public class FileHashEntity : EntityBase
{
    public string FileId { get; set; }
    public byte[] Hash { get; set; }
    public long Length { get; set; }
    public DateTime Time { get; set; }
}