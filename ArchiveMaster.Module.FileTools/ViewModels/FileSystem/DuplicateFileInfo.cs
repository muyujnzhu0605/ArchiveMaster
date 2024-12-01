using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels.FileSystem;

public partial class DuplicateFileInfo : SimpleFileInfo
{
    [ObservableProperty]
    private string existedFilePath;

    public DuplicateFileInfo(FileInfo file, string topDir,string existedFilePath) : base(file, topDir)
    {
        ExistedFilePath = existedFilePath;
    }
}