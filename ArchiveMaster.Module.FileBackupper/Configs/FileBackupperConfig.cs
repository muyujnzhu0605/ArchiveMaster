using ArchiveMaster.Enums;

namespace ArchiveMaster.Configs;

public partial class FileBackupperConfig : ConfigBase
{
    public List<BackupTask> Tasks { get; set; } = new List<BackupTask>();

    public override void Check()
    {
    }
}