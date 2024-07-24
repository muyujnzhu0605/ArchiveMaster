using ArchiveMaster.Model;
using ArchiveMaster.Enums;

namespace ArchiveMaster.Configs
{
    public partial class Step2Config : OfflineSyncStepConfigBase
    {
        public string BlackList { get; set; }
        public bool BlackListUseRegex { get; set; }
        public ExportMode ExportMode { get; set; } = ExportMode.Copy;
        public string LocalDir { get; set; }
        public string PatchDir { get; set; }
        public string OffsiteSnapshot { get; set; }
        public List<LocalAndOffsiteDir> MatchingDirs { get; set; }
    }
}