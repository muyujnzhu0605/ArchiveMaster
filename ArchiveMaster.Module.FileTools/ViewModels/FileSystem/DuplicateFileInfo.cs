using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels.FileSystem;

public partial class DuplicateFileInfo : SimpleFileInfo
{
    [ObservableProperty]
    private SimpleFileInfo existedFile;

    public DuplicateFileInfo(FileInfo file, string topDir, SimpleFileInfo existedFile) : base(file, topDir)
    {
        ExistedFile = existedFile;
    }
}