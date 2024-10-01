using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ArchiveMaster.Configs;

public partial class BackupperTask : ObservableObject
{
    [ObservableProperty]
    private string backupDirs;

    [ObservableProperty]
    private string blackList;

    [ObservableProperty]
    private bool byTimeInterval = true;

    [ObservableProperty]
    private bool byWatching = true;

    [ObservableProperty]
    private ObservableCollection<string> includingFiles = new ObservableCollection<string>();

    [ObservableProperty]
    private ObservableCollection<string> includingFolders = new ObservableCollection<string>();

    [ObservableProperty]
    private string name = "新备份任务";

    [ObservableProperty]
    private TimeSpan timeInterval = TimeSpan.FromHours(1);
}