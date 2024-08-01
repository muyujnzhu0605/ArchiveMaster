namespace ArchiveMaster.Configs;

public class FilesLocationRepairerConfig : ConfigBase
{
    public string TemplateDir { get; set; }
    public string SourceDir { get; set; }
    public string TargetDir { get; set; }
    public bool CompareTime { get; set; }
    public bool CompareLength { get; set; }
    public bool CompareName { get; set; }
    public string BlackList { get; set; }
    public bool BlackListUseRegex { get; set; }
    public int MaxTimeToleranceSecond { get; set; }
    public bool Copy { get; set; }
}