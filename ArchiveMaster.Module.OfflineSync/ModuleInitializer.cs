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
using ArchiveMaster.Utilities;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster
{
    public class OfflineSyncModuleInitializer : IModuleInitializer
    {
        private readonly string baseUrl = "avares://ArchiveMaster.Module.OfflineSync/Assets/";
        public string ModuleName => "异地备份离线同步";
        public int Order => 3;

        public void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<Step1ViewModel>();
            services.AddTransient<Step2ViewModel>();
            services.AddTransient<Step3ViewModel>();

            services.AddTransient<Step1Panel>();
            services.AddTransient<Step2Panel>();
            services.AddTransient<Step3Panel>();

            services.AddTransient<Step1Utility>();
            services.AddTransient<Step2Utility>();
            services.AddTransient<Step3Utility>();
        }

        public IList<ConfigInfo> Configs =>
        [
            new ConfigInfo(typeof(OfflineSyncConfig))
        ];

        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(Step1Panel), "制作异地快照", "在异地计算机创建所需要的目录快照", baseUrl + "snapshot.svg"),
                new ToolPanelInfo(typeof(Step2Panel), "本地生成补丁", "在本地计算机生成与异地的差异文件的补丁包", baseUrl + "patch.svg"),
                new ToolPanelInfo(typeof(Step3Panel), "异地同步", "在异地应用补丁包，实现数据同步", baseUrl + "update.svg")
            },
            GroupName = ModuleName,
            MenuItems =
            {
                new ModuleMenuItemInfo("生成测试数据", new AsyncRelayCommand(async () =>
                {
                    var folders = await WeakReferenceMessenger.Default.Send(new GetStorageProviderMessage())
                        .StorageProvider
                        .OpenFolderPickerAsync(new FolderPickerOpenOptions());
                    if (folders.Count > 0)
                    {
                        var folder = folders[0].TryGetLocalPath();
                        await TestUtility.CreateSyncTestFilesAsync(folder);
                    }
                })),
                new ModuleMenuItemInfo("自动化测试", new AsyncRelayCommand(async () =>
                {
                    try
                    {
                        await TestUtility.TestAllAsync();

                        await WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
                        {
                            Type = CommonDialogMessage.CommonDialogType.Ok,
                            Title = "自动化测试",
                            Message = "通过测试",
                        }).Task;
                    }
                    catch (Exception ex)
                    {
                        await WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
                        {
                            Type = CommonDialogMessage.CommonDialogType.Error,
                            Title = "测试未通过",
                            Exception = ex
                        }).Task;
                    }
                }))
            }
        };

        public void RegisterMessages(Visual visual)
        {
         
        }
    }
}