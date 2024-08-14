namespace ArchiveMaster.ViewModels;

public class ExifTimeFileInfo:SimpleFileInfo
{
    public ExifTimeFileInfo(FileInfo file) : base(file)
    {
        
    }
    public DateTime? ExifTime { get; set; }
}