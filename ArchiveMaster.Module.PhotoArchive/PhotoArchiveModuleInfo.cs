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
        public const string PHOTO_SLIMMING_GROUP = "PhotoSlimming";
        public string ModuleName => "照片工具";

        public int Order => 2;

        public IList<ConfigMetadata> Configs =>
        [
            new ConfigMetadata(typeof(TimeClassifyConfig)),
            new ConfigMetadata(typeof(RepairModifiedTimeConfig)),
            new ConfigMetadata(typeof(UselessJpgCleanerConfig)),
            new ConfigMetadata(typeof(PhotoSlimmingConfig), PHOTO_SLIMMING_GROUP),
        ];

        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(TimeClassifyPanel), typeof(TimeClassifyViewModel), "根据时间段归档",
                    "识别目录中相同时间段的文件，将它们移动到相同的新目录中",
                    baseUrl + "archive.svg"),
                new ToolPanelInfo(typeof(UselessJpgCleanerPanel), typeof(UselessJpgCleanerViewModel), "删除多余JPG",
                    "删除目录中存在同名RAW文件的JPG文件", baseUrl + "jpg.svg"),
                new ToolPanelInfo(typeof(RepairModifiedTimePanel), typeof(RepairModifiedTimeViewModel), "修复文件修改时间",
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
            typeof(UselessJpgCleanerService),
            typeof(TimeClassifyService)
        ];

        private readonly string baseUrl = "avares://ArchiveMaster.Module.PhotoArchive/Assets/";
    }
}