using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class RebuildConfig:ConfigBase 
{
    [ObservableProperty]
    private string discDirs;

    [ObservableProperty]
    private string targetDir;

    [ObservableProperty]
    private bool skipIfExisted;

    [ObservableProperty]
    private int maxTimeToleranceSecond;

    [ObservableProperty]
    private bool checkOnly;
}