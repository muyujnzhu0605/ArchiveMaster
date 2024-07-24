namespace ArchiveMaster.Configs
{
    public class Step1Config : OfflineSyncStepConfigBase
    {
        public string OutputFile { get; set; }
        public List<string> SyncDirs { get; set; }
    }
}