using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class DuplicateFileCleanupConfig : ConfigBase
{
    [ObservableProperty]
    private string cleaningDir;

    [ObservableProperty]
    private string referenceDir;

    [ObservableProperty]
    private bool cleanUpSelf;

    [ObservableProperty]
    private bool cleanUpByReference;

    [ObservableProperty]
    private bool compareTime = true;

    [ObservableProperty]
    private bool compareLength = true;

    [ObservableProperty]
    private bool compareName = true;

    [ObservableProperty]
    private int timeToleranceSecond;

    public override void Check()
    {
        if (!CleanUpSelf && !CleanUpByReference)
        {
            throw new Exception("至少选择一项清理内容");
        }

        CheckDir(CleaningDir, "待清理目录");
        if (CleanUpByReference)
        {
            CheckDir(ReferenceDir, "参考目录");
        }
    }
}