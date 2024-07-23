using ArchiveMaster.Configs;
using ArchiveMaster.UI;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMaster
{
    public class ModuleInitializer : IModuleInitializer
    {
        public string ModuleName { get; } = "异地备份离线同步";

        public void RegisterConfigs()
        {
            AppConfig.RegisterConfig<OfflineSyncConfig>(nameof(OfflineSyncConfig));

        }

        public void RegisterViews()
        {
            string baseUrl = "avares://ArchiveMaster.Module.OfflineSync/Assets/";
            ToolPanelInfo.Register<Step1Panel>(ModuleName, "制作异地快照", "在异地计算机创建所需要的目录快照", baseUrl + "encrypt.svg");
        }
    }
}
