using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiveMaster.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArchiveMaster
{
    public class FileBackupperModuleInitializer : IModuleInitializer, IServiceModuleInitializer
    {
        public string ModuleName => "文件备份工具";

        public int Order =>
#if DEBUG
            0;
#else
            5;
#endif

        public IList<ConfigInfo> Configs =>
        [
            new ConfigInfo(typeof(FileBackupperConfig)),
        ];

        public void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<BackupperTasksViewModel>();

            services.AddTransient<BackupperTasksPanel>();
        }

        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(BackupperTasksPanel), "任务列表", "备份任务的管理", baseUrl + "disc.svg"),
                //
            },
            GroupName = ModuleName
        };


        public void RegisterMessages(Visual visual)
        {
        }

        private readonly string baseUrl = "avares://ArchiveMaster.Module.FileBackupper/Assets/";

        public void AddServices(IServiceCollection services)
        {
        }
    }
}