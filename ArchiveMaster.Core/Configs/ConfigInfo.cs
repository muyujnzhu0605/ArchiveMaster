using System.Diagnostics;
using System.Text.Json.Serialization;

namespace ArchiveMaster.Configs
{
    [DebuggerDisplay("{Key}")]
    public class ConfigInfo
    {
        public ConfigInfo()
        {
        }

        public ConfigInfo(Type type)
        {
            Type = type;
            Key = type.Name;
        }

        [JsonIgnore]
        public Type Type { get; set; }
        public string Key { get; set; }
    }
}
