using ArchiveMaster.Models;

namespace ArchiveMaster.ViewModels.FileSystem;

public class BackupFile : TreeFileInfo
{
    public BackupFile(BackupFileEntity record)
    {
        Entity = record;
        Path = record.RawFileRelativePath;
        Name = System.IO.Path.GetFileName(record.RawFileRelativePath);

        Time = record.Time;
        Length = record.Length;
    }

    public BackupFileEntity Entity { get; set; }
}