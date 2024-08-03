namespace ArchiveMaster.Configs;

public class RebuildConfig
{
    public string DiscDirs { get; set; }
    public string TargetDir { get; set; }
    public bool SkipIfExisted { get; set; }
    public int MaxTimeToleranceSecond { get; set; }

    public bool CheckOnly { get; set; }
}