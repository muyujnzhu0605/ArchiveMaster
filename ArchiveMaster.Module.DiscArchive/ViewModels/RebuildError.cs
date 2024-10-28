namespace ArchiveMaster.ViewModels;

public class RebuildError(FileSystem.DiscFile file, string error)
{
    public string Error { get; set; } = error;
    public FileSystem.DiscFile File { get; set; } = file;
}