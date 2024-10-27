using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels.FileSystem;

public partial class RenameFileInfo : SimpleFileInfo
{
    public RenameFileInfo(FileSystemInfo fileOrDir, string topDir) : base(fileOrDir, topDir)
    {
    }

    public RenameFileInfo() : base()
    {
    }

    [ObservableProperty]
    private bool isMatched;

    [ObservableProperty]
    private string newName;

    [ObservableProperty]
    private string newPath;

    [ObservableProperty]
    private string tempPath;
}