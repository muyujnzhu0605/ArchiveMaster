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

namespace ArchiveMaster
{
    public class FileToolsModuleInitializer : IModuleInitializer
    {
        public string ModuleName => "文件目录工具";

        public int Order => 1;

        public IList<ConfigInfo> Configs =>
        [
            new ConfigInfo(typeof(EncryptorConfig)),
            new ConfigInfo(typeof(DirStructureSyncConfig)),
            new ConfigInfo(typeof(DirStructureCloneConfig)),
            new ConfigInfo(typeof(RenameConfig))
        ];

        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(EncryptorPanel), "文件加密解密", "使用AES加密方法，对文件进行加密或解密", baseUrl + "encrypt.svg"),
                new ToolPanelInfo(typeof(DirStructureSyncPanel), "目录结构同步", "以一个目录为模板，将另一个目录中的文件同步到与模板内相同文件一直的位置",
                    baseUrl + "sync.svg"),
                new ToolPanelInfo(typeof(DirStructureClonePanel), "目录结构克隆", "以一个目录为模板，导出一个新的稀疏文件目录，或包含文件结构的JSON文件",
                    baseUrl + "directory.svg"),
                new ToolPanelInfo(typeof(RenamePanel), "批量重命名", "批量对一个目录中的文件或文件夹按规则进行重命名操作",
                    baseUrl + "rename.svg"),
            },
            GroupName = ModuleName
        };


        public void RegisterMessages(Visual visual)
        {
        }

        private readonly string baseUrl = "avares://ArchiveMaster.Module.FileTools/Assets/";

        public void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<EncryptorViewModel>();
            services.AddTransient<DirStructureCloneViewModel>();
            services.AddTransient<DirStructureSyncViewModel>();
            services.AddTransient<RenameViewModel>();
            
            services.AddTransient<EncryptorPanel>();
            services.AddTransient<DirStructureClonePanel>();
            services.AddTransient<DirStructureSyncPanel>();
            services.AddTransient<RenamePanel>();
            
            services.AddTransient<EncryptorUtility>();
            services.AddTransient<DirStructureCloneUtility>();
            services.AddTransient<DirStructureSyncUtility>();
            services.AddTransient<RenameUtility>();  
            
        }
    }
}