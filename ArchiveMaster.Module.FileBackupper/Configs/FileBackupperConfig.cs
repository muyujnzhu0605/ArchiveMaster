using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class FileBackupperConfig : ConfigBase
{
    [ObservableProperty]
    private bool enableBackgroundBackup;
    public List<BackupTask> Tasks { get; set; } = new List<BackupTask>();

    public override void Check()
    {
    }
}