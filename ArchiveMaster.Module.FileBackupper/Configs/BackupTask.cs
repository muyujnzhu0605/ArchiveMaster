using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ArchiveMaster.Configs;

public partial class BackupTask : ConfigBase
{
    [ObservableProperty]
    private string backupDir;

    [ObservableProperty]
    private string blackList;
    
    [ObservableProperty]
    private bool blackListUseRegex;

    [ObservableProperty]
    private bool byTimeInterval = true;

    [ObservableProperty]
    private bool byWatching = true;

    [ObservableProperty]
    private string sourceDir;

    [ObservableProperty]
    private string name = "新备份任务";

    [ObservableProperty]
    private TimeSpan timeInterval = TimeSpan.FromHours(1);

    public override void Check()
    {
        
    }
}