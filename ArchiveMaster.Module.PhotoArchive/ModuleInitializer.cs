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
    public class PhotoArchiveModuleInitializer : IModuleInitializer
    {
        public string ModuleName => "照片工具";
        
        public int Order => 2;

        public void RegisterStyles()
        {
        }

        public IList<ConfigInfo> Configs =>
        [
            new ConfigInfo(typeof(TimeClassifyConfig)),
            new ConfigInfo(typeof(RepairModifiedTimeConfig)),
            new ConfigInfo(typeof(UselessJpgCleanerConfig)),
            new ConfigInfo(typeof(List<PhotoSlimmingConfig>), nameof(PhotoSlimmingConfig)),
        ];
        
        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(TimeClassifyPanel), "根据时间段归档", "识别目录中相同时间段的文件，将它们移动到相同的新目录中",
                    baseUrl + "archive.svg"),
                new ToolPanelInfo(typeof(UselessJpgCleanerPanel), "删除多余JPG", "删除目录中存在同名RAW文件的JPG文件",
                    baseUrl + "jpg.svg"),
                new ToolPanelInfo(typeof(RepairModifiedTimePanel), "修复文件修改时间",
                    "寻找EXIF信息中的拍摄时间与照片修改时间不同的文件，将修改时间更新闻EXIF时间", baseUrl + "time.svg"),
                new ToolPanelInfo(typeof(PhotoSlimmingPanel), "创建照片集合副本", "复制或压缩照片，用于生成更小的照片集副本", baseUrl + "zip.svg"),
            },
            GroupName = ModuleName
        };

        public void RegisterMessages(Visual visual)
        {
        }

        private readonly string baseUrl = "avares://ArchiveMaster.Module.PhotoArchive/Assets/";
    }
}