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
    
    partial void OnSourceDirChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(TargetDir) || oldValue == TargetDir)
        {
            TargetDir = newValue;
        }
    }
}