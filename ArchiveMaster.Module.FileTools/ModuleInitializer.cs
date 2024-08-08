using ArchiveMaster.Configs;
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
        public string ModuleName => "文件目录工具";

        public int Order => 1;

        public IList<ConfigInfo> Configs =>
        [
            new ConfigInfo(typeof(EncryptorConfig)),
            new ConfigInfo(typeof(DirStructureSyncConfig)),
            new ConfigInfo(typeof(DirStructureCloneConfig))
        ];

        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(EncryptorPanel), "文件加密解密", "使用AES加密方法，对文件进行加密或解密", baseUrl + "encrypt.svg"),
                new ToolPanelInfo(typeof(DirStructureSyncPanel), "目录结构同步", "以一个目录为模板，将另一个目录中的文件同步到与模板内相同文件一直的位置",
                    baseUrl + "sync.svg"),
                new ToolPanelInfo(typeof(DirStructureClonePanel), "目录结构克隆", "以一个目录为模板，生成一个新的目录，目录中文件与模板一致，但大小为0",
                    baseUrl + "directory.svg"),
            },
            GroupName = ModuleName
        };


        public void RegisterMessages(Visual visual)
        {
        }

        private readonly string baseUrl = "avares://ArchiveMaster.Module.FileTools/Assets/";
    }
}