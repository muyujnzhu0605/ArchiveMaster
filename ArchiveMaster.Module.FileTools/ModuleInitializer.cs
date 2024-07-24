using ArchiveMaster.Configs;
using ArchiveMaster.UI;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMaster
{
    public class ModuleInitializer : IModuleInitializer
    {
        public string ModuleName { get; } = "文件（夹）工具";

        public void RegisterConfigs()
        {
            AppConfig.RegisterConfig<EncryptorConfig>(nameof(EncryptorConfig));

        }

        public void RegisterMessages(Visual visual)
        {
        }

        public void RegisterViews()
        {
            string baseUrl = "avares://ArchiveMaster.Module.FileTools/Assets/";
            ToolPanelInfo.Register<EncryptorPanel>(ModuleName, "文件加密解密", "使用AES加密方法，对文件进行加密或解密", baseUrl + "encrypt.svg");
        }
    }
}
