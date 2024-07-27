using ArchiveMaster.Enums;

namespace ArchiveMaster.Configs
{
    public class Step3Config : OfflineSyncStepConfigBase
    {
        public DeleteMode DeleteMode { get; set; } = DeleteMode.MoveToDeletedFolder;

        public string PatchDir { get; set; }
    }
}