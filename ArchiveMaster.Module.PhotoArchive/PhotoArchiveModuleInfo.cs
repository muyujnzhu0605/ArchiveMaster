using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiveMaster.Models;
using ArchiveMaster.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster
{
    public class PhotoArchiveModuleInfo : IModuleInfo
    {
        public string ModuleName => "文件目录工具";

        public int Order => 2;

        public IList<ConfigMetadata> Configs =>
        [
            new ConfigMetadata(typeof(TimeClassifyConfig)),
            new ConfigMetadata(typeof(RepairModifiedTimeConfig)),
            new ConfigMetadata(typeof(TwinFileCleanerConfig)),
            new ConfigMetadata(typeof(PhotoSlimmingConfig)),
        ];

        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(TimeClassifyPanel), typeof(TimeClassifyViewModel), "根据时间段归档",
                    "识别目录中相同时间段的文件，将它们移动到相同的新目录中",
                    baseUrl + "archive.svg"),
                new ToolPanelInfo(typeof(TwinFileCleanerPanel), typeof(TwinFileCleanerViewModel), "同名异后缀文件清理",
                    "当目录中存在某后缀文件（如.dng）时，自动删除同名不同后缀的关联文件（如.jpg）", baseUrl + "jpg.svg"),
                new ToolPanelInfo(typeof(RepairModifiedTimePanel), typeof(RepairModifiedTimeViewModel), "修复照片修改时间",
                    "寻找EXIF信息中的拍摄时间与照片修改时间不同的文件，将修改时间更新闻EXIF时间", baseUrl + "time.svg"),
                new ToolPanelInfo(typeof(PhotoSlimmingPanel), typeof(PhotoSlimmingViewModel), "创建照片集合副本",
                    "复制或压缩照片，用于生成更小的照片集副本", baseUrl + "zip.svg"),
            },
            GroupName = ModuleName
        };

        public IList<Type> BackgroundServices { get; }
        public IList<Type> SingletonServices { get; }

        public IList<Type> TransientServices { get; } =
        [
            typeof(PhotoSlimmingService),
            typeof(RepairModifiedTimeService),
            typeof(TwinFileCleanerService),
            typeof(TimeClassifyService)
        ];

        private readonly string baseUrl = "avares://ArchiveMaster.Module.PhotoArchive/Assets/";
    }
}