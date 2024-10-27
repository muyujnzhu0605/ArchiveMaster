using ArchiveMaster.Models;
using FzLib.DataStorage.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArchiveMaster.Configs
{
    public class OfflineSyncConfig
    {
        public static readonly int MaxTimeTolerance = 3;

        public Dictionary<string, SingleConfig> ConfigCollection { get; set; } = new Dictionary<string, SingleConfig>()
        {
            ["默认"] = new SingleConfig()
        };

        [JsonIgnore]
        public SingleConfig CurrentConfig
        {
            get
            {
                if (CurrentConfigName == null)
                {
                    return null;
                }

                if (ConfigCollection.TryGetValue(CurrentConfigName, out SingleConfig value))
                {
                    return value;
                }
                else
                {
                    ConfigCollection.Add(CurrentConfigName, new SingleConfig());
                    return ConfigCollection[CurrentConfigName];
                }
            }
        }

        public string CurrentConfigName { get; set; } = "默认";
        public string DeleteDirName { get; set; } = "异地备份离线同步-删除的文件";
    }

    public class SingleConfig
    {
        public Step1Config Step1 { get; set; } = new Step1Config();
        public Step2Config Step2 { get; set; } = new Step2Config();
        public Step3Config Step3 { get; set; } = new Step3Config();
    }
}