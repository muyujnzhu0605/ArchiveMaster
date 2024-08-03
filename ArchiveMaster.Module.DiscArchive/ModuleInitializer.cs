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
        public string ModuleName => "光盘归档工具";
        
        public int Order => 4;

        public IList<ConfigInfo> Configs =>
        [
        ];

        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                // new ToolPanelInfo(typeof(EncryptorPanel), "打包到光盘", "将文件按照修改时间顺序，根据光盘最大容量制作成若干文件包", baseUrl + "disc.svg"),
                // new ToolPanelInfo(typeof(EncryptorPanel), "从光盘重建", "从备份的光盘冲提取文件并恢复为原始目录结构", baseUrl + "rebuild.svg"),
                //
            },
            GroupName = ModuleName
        };


        public void RegisterMessages(Visual visual)
        {
        }

        private readonly string baseUrl = "avares://ArchiveMaster.Module.DiscArchive/Assets/";
    }
}