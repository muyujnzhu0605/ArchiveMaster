using ArchiveMaster.Models;
using ArchiveMaster.ViewModels;
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


        public Dictionary<string, SingleConfig> ConfigCollection { get; set; } = new Dictionary<string, SingleConfig>();
        
        [JsonIgnore]
        public SingleConfig CurrentConfig
        {
            get
            {
                if (ConfigCollection.ContainsKey(CurrentConfigName))
                {
                    return ConfigCollection[CurrentConfigName];
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
        public Step1ViewModel Step1 { get; set; } = new  Step1ViewModel();
        public Step2ViewModel Step2 { get; set; } = new Step2ViewModel();
        public Step3ViewModel Step3 { get; set; } = new Step3ViewModel();
    }
}
