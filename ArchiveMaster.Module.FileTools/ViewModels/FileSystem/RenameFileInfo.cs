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
    
    /// <summary>
    /// 经过了唯一文件名处理，即为了保证文件名不重复，新的文件名并非按照规则进行处理后的文件名
    /// </summary>
    [ObservableProperty]
    private bool hasUniqueNameProcessed;

    public string TempPath { get; set; }

    public string GetNewPath()
    {
        return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), NewName);
    }
}