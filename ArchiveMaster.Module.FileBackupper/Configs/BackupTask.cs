using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using ArchiveMaster.Enums;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Configs;

public partial class BackupTask : ConfigBase, ICloneable
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

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
    private bool enableAutoBackup = true;

    [ObservableProperty]
    private bool isDefaultVirtualBackup;

    [ObservableProperty] [property: JsonIgnore]
    private DateTime lastBackupTime;

    [ObservableProperty] [property: JsonIgnore]
    private DateTime lastFullBackupTime;

    [ObservableProperty] [property: JsonIgnore]
    private string message;

    [ObservableProperty]
    private string name = "新备份任务";

    [ObservableProperty] [property: JsonIgnore]
    private int snapshotCount;

    [ObservableProperty]
    private string sourceDir;

    [ObservableProperty] [property: JsonIgnore]
    private BackupTaskStatus status = BackupTaskStatus.Ready;

    [ObservableProperty]
    private TimeSpan timeInterval = TimeSpan.FromHours(1);

    public override void Check()
    {
        CheckDir(SourceDir, "需要备份的目录");
        CheckDir(BackupDir, "备份文件存放目录");
        CheckEmpty(Name, "备份任务名");
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}