using ArchiveMaster.Configs;
using ArchiveMaster.Messages;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Dialogs;
using FzLib.Avalonia.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiveMaster.Services;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster
{
    public class TestModuleInitializer : IModuleInitializer
    {
        private readonly string baseUrl = "avares://ArchiveMaster.Module.Test/Assets/";
        public string ModuleName => "测试";
        public int Order => -100;

        public void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<FileFilterTestPanel>();
        }

        public IList<ConfigInfo> Configs =>
        [
        ];

        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(FileFilterTestPanel), "文件筛选测试", "测试FileFilter功能", baseUrl + "test.svg"),
            },
            GroupName = ModuleName,
        };

        public void RegisterMessages(Visual visual)
        {
         
        }
    }
}