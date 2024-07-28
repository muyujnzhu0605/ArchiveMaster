using ArchiveMaster.Configs;
using ArchiveMaster.Messages;
using ArchiveMaster.UI;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiveMaster.Utility;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;

namespace ArchiveMaster
{
    public class ModuleInitializer : IModuleInitializer
    {
        private readonly string baseUrl = "avares://ArchiveMaster.Module.OfflineSync/Assets/";
        public string ModuleName => "异地备份离线同步";

        public IList<ConfigInfo> Configs => [
            new ConfigInfo(type: typeof(OfflineSyncConfig), key: nameof(OfflineSyncConfig))
        ];
        public ToolPanelGroupInfo Views => new ToolPanelGroupInfo()
        {
            Panels =
            {
                new ToolPanelInfo(typeof(Step1Panel), "1.制作异地快照","在异地计算机创建所需要的目录快照", baseUrl + "snapshot.svg"),
                new ToolPanelInfo(typeof(Step2Panel), "2.本地生成补丁","在本地计算机生成与异地的差异文件的补丁包", baseUrl + "patch.svg"),
                new ToolPanelInfo(typeof(Step3Panel), "3.异地同步","在异地应用补丁包，实现数据同步", baseUrl + "update.svg")
            },
            GroupName = ModuleName,
            MenuItems =
            {
                new ModuleMenuItemInfo("生成测试数据", new RelayCommand(() =>
                {
                    //TestUtility .CreateSyncTestFilesAsync()
                }))
            }
        };
        public void RegisterMessages(Visual visual)
        {
            WeakReferenceMessenger.Default.Register<InputDialogMessage>(visual, async (_, m) =>
            {
                try
                {
                    object result = m.Type switch
                    {
                        InputDialogMessage.InputDialogType.Text => await visual.ShowInputTextDialogAsync(m.Title,
                            m.Message, m.DefaultValue as string, m.Watermark, m.Validation),
                        InputDialogMessage.InputDialogType.Integer => await visual.ShowInputNumberDialogAsync(m.Title,
                            m.Message, (int)m.DefaultValue, m.Watermark),
                        InputDialogMessage.InputDialogType.Float => await visual.ShowInputNumberDialogAsync(m.Title,
                            m.Message, (double)m.DefaultValue, m.Watermark),
                        InputDialogMessage.InputDialogType.Password => await visual.ShowInputPasswordDialogAsync(
                            m.Title, m.Message, m.Watermark, m.Validation),
                        InputDialogMessage.InputDialogType.MultipleLinesText => await
                            visual.ShowInputMultiLinesTextDialogAsync(m.Title, m.Message, 3, 10,
                                m.DefaultValue as string, m.Watermark, m.Validation),
                        _ => throw new InvalidEnumArgumentException()
                    };
                    m.SetResult(result);
                }
                catch (Exception ex)
                {
                    m.SetException(ex);
                }
            });
        }

    }
}