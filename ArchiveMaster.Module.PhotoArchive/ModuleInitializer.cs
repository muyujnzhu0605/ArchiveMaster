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
        public string ModuleName { get; } = "照片工具";

        public void RegisterConfigs()
        {
            AppConfig.RegisterConfig<TimeClassifyConfig>(nameof(TimeClassifyConfig));
            AppConfig.RegisterConfig<RepairModifiedTimeConfig>(nameof(RepairModifiedTimeConfig));
            AppConfig.RegisterConfig<UselessJpgCleanerConfig>(nameof(UselessJpgCleanerConfig));
            AppConfig.RegisterConfig<List<PhotoSlimmingConfig>>(nameof(PhotoSlimmingConfig));

        }

        public void RegisterViews()
        {
            string baseUrl = "avares://ArchiveMaster.Module.PhotoArchive/Assets/";
            ToolPanelInfo.Register<TimeClassifyPanel>(ModuleName, "根据时间段归档", "识别目录中相同时间段的文件，将它们移动到相同的新目录中", baseUrl + "archive.svg");
            ToolPanelInfo.Register<UselessJpgCleanerPanel>(ModuleName, "删除多余JPG", "删除目录中存在同名RAW文件的JPG文件", baseUrl + "jpg.svg");
            ToolPanelInfo.Register<RepairModifiedTimePanel>(ModuleName, "修复文件修改时间", "寻找EXIF信息中的拍摄时间与照片修改时间不同的文件，将修改时间更新闻EXIF时间", baseUrl + "time.svg");
            ToolPanelInfo.Register<PhotoSlimmingPanel>(ModuleName, "创建照片集合副本", "复制或压缩照片，用于生成更小的照片集副本", baseUrl + "zip.svg");
        }
    }
}
