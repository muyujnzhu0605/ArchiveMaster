using ArchiveMaster.Enums;

namespace ArchiveMaster.Configs;

public class PackingConfig
{
    public string SourceDir { get; set; }
    public string TargetDir { get; set; }
    public DateTime EarliestTime { get; set; }
    public string BlackList { get; set; }
    public bool BlackListUseRegex { get; set; }
    public PackingType PackingType { get; set; }
    public int DiscSizeMB { get; set; }
    public int MaxDiscCount { get; set; }
}