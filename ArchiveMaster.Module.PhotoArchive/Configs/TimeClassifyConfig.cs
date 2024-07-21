using System;

namespace ArchiveMaster.Configs
{
    public class TimeClassifyConfig: ConfigBase
    {
        public string Dir { get; set; }
        public TimeSpan MinTimeInterval { get; set; } = TimeSpan.FromMinutes(60);
    }
}
