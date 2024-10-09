using ArchiveMaster.Models;

namespace ArchiveMaster.ViewModels.FileSystem;

public class BackupFile : TreeFileInfo
{
    public BackupFile(FileRecordEntity record)
    {
        RecordEntity = record;
        Path = record.RawFileRelativePath;
        Name = System.IO.Path.GetFileName(record.RawFileRelativePath);
        if (record.PhysicalFile != null)
        {
            Time = record.PhysicalFile.Time;
            Length = record.PhysicalFile.Length;
        }
    }

    public FileRecordEntity RecordEntity { get; set; }
}