using ArchiveMaster.Enums;

namespace ArchiveMaster.Configs;

public partial class FileBackupperConfig : ConfigBase
{
    public List<BackupperTask> Tasks { get; set; } = new List<BackupperTask>();

    public override void Check()
    {
    }
}