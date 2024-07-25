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
        public string ModuleName => "文件（夹）工具";

        public IList<ConfigInfo> Configs => [new ConfigInfo(typeof(EncryptorConfig))];

        public IList<ToolPanelInfo> Views =>
        [
            new ToolPanelInfo(typeof(EncryptorPanel), ModuleName, "文件加密解密", "使用AES加密方法，对文件进行加密或解密",
                baseUrl + "encrypt.svg")
        ];

        public IList<Uri> StyleUris { get; }

        public void RegisterMessages(Visual visual)
        {
        }

        private readonly string baseUrl = "avares://ArchiveMaster.Module.FileTools/Assets/";
    }
}