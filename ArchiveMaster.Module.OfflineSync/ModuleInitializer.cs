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

namespace ArchiveMaster
{
    public class ModuleInitializer : IModuleInitializer
    {
        public string ModuleName { get; } = "异地备份离线同步";

        public void RegisterConfigs()
        {
            AppConfig.RegisterConfig<OfflineSyncConfig>(nameof(OfflineSyncConfig));

        }

        public void RegisterMessages(Visual visual)
        {
            WeakReferenceMessenger.Default.Register<InputDialogMessage>(visual, async (_, m) =>
            {
                try
                {
                    object result = m.Type switch
                    {
                        InputDialogMessage.InputDialogType.Text =>await visual.ShowInputTextDialogAsync(m.Title, m.Message, m.DefaultValue as string, m.Watermark, m.Validation),
                        InputDialogMessage.InputDialogType.Integer => await visual.ShowInputNumberDialogAsync(m.Title, m.Message, (int)m.DefaultValue, m.Watermark),
                        InputDialogMessage.InputDialogType.Float => await visual.ShowInputNumberDialogAsync(m.Title, m.Message, (double)m.DefaultValue, m.Watermark),
                        InputDialogMessage.InputDialogType.Password => await visual.ShowInputPasswordDialogAsync(m.Title, m.Message, m.Watermark, m.Validation),
                        InputDialogMessage.InputDialogType.MultipleLinesText => await visual.ShowInputMultiLinesTextDialogAsync(m.Title, m.Message, 3, 10, m.DefaultValue as string, m.Watermark, m.Validation),
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

        public void RegisterViews()
        {
            string baseUrl = "avares://ArchiveMaster.Module.OfflineSync/Assets/";
            ToolPanelInfo.Register<Step1Panel>(ModuleName, "制作异地快照", "在异地计算机创建所需要的目录快照", baseUrl + "encrypt.svg");
        }
    }
}
