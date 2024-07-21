using System.Diagnostics;

namespace ArchiveMaster.Configs
{
    [DebuggerDisplay("{Key}")]
    internal class ConfigInfo
    {
        public Type Type { get; set; }
        public string Key { get; set; }
        public object Config { get; set; }
    }
}
