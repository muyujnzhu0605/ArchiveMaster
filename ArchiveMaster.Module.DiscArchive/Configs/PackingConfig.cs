using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class PackingConfig : ConfigBase
{
    [ObservableProperty]
    private string sourceDir;

    [ObservableProperty]
    private string targetDir;

    [ObservableProperty]
    private DateTime earliestTime = DateTime.MinValue;

    [ObservableProperty]
    private string blackList;

    [ObservableProperty]
    private bool blackListUseRegex;

    [ObservableProperty]
    private PackingType packingType = PackingType.Copy;

    [ObservableProperty]
    private int discSizeMB = 23500;

    [ObservableProperty]
    private int maxDiscCount = 10000;
}