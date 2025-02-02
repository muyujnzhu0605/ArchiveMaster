using System.Diagnostics;
using System.Text.Json.Serialization;

namespace ArchiveMaster.Configs
{
    [DebuggerDisplay("{Key}")]
    public class ConfigMetadata
    {
        public ConfigMetadata()
        {
        }

        public ConfigMetadata(Type type, string group = null)
        {
            Type = type;
            Key = type.Name;
            Group = group ?? type.Name;
        }

        public Type Type { get; set; }

        public string Key { get; set; }

        /// <summary>
        /// 启用多版本配置时的版本组名。同一个组名内的配置共享一组版本号。
        /// </summary>
        /// <returns></returns>
        public string Group { get; set; }
    }
}