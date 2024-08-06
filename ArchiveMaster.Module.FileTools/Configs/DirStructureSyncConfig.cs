using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class DirStructureSyncConfig : ConfigBase
{
    [ObservableProperty]
    private string templateDir;

    [ObservableProperty]
    private string sourceDir;

    [ObservableProperty]
    private string targetDir;

    [ObservableProperty]
    private bool compareTime;

    [ObservableProperty]
    private bool compareLength;

    [ObservableProperty]
    private bool compareName;

    [ObservableProperty]
    private string blackList;

    [ObservableProperty]
    private bool blackListUseRegex;

    [ObservableProperty]
    private int maxTimeToleranceSecond;

    [ObservableProperty]
    private bool copy;
    
    public override void Check()
    {
        CheckDir(SourceDir,"源目录");
        CheckDir(SourceDir,"模板目录");
        CheckEmpty(SourceDir,"目标目录");
    }
    
    partial void OnSourceDirChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(TargetDir) || oldValue == TargetDir)
        {
            TargetDir = newValue;
        }
    }
}