using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class DuplicateFileCleanupConfig : ConfigBase
{
    [ObservableProperty]
    private string cleaningDir;

    [ObservableProperty]
    private bool compareLength = true;

    [ObservableProperty]
    private bool compareName = true;

    [ObservableProperty]
    private bool compareTime = true;

    [ObservableProperty]
    private string referenceDir;
    
    [ObservableProperty]
    private int timeToleranceSecond;

    [ObservableProperty]
    private string recycleBin;

    public override void Check()
    {
        CheckDir(CleaningDir, "待清理目录");
        CheckDir(ReferenceDir, "参考目录");
        CheckDir(RecycleBin, "删除文件位置");
    }
}