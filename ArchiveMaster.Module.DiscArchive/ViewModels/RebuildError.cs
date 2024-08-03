namespace ArchiveMaster.ViewModels;

public class RebuildError(DiscFile file, string error)
{
    public string Error { get; set; } = error;
    public DiscFile File { get; set; } = file;
}