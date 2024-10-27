using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using ArchiveMaster.Views;
using System.Collections;
using System.Collections.ObjectModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Utilities;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels
{
    public partial class Step3ViewModel(AppConfig appConfig) : OfflineSyncViewModelBase<Step3Utility, Step3Config, SyncFileInfo>(appConfig)
    {
        public IEnumerable DeleteModes => Enum.GetValues<DeleteMode>();

        public override Step3Config Config =>
            Services.Provider.GetRequiredService<OfflineSyncConfig>().CurrentConfig?.Step3;
        protected override Step3Utility CreateUtilityImplement()
        {
            return new Step3Utility(Config, appConfig);
        }

        protected override Task OnInitializedAsync()
        {
            Files = new ObservableCollection<SyncFileInfo>(Utility.UpdateFiles);
            return base.OnInitializedAsync();
        }

        protected override async Task OnExecutedAsync(CancellationToken token)
        {
            Utility.AnalyzeEmptyDirectories(token);
            if (Utility.DeletingDirectories.Count != 0)
            {
                var result = await this.SendMessage(new CommonDialogMessage()
                {
                    Title = "删除空目录",
                    Message = $"有{Utility.DeletingDirectories.Count}个已不存在于本地的空目录，是否删除？",
                    Detail = string.Join(Environment.NewLine,
                        Utility.DeletingDirectories.Select(p => p.Path)),
                    Type = CommonDialogMessage.CommonDialogType.YesNo
                }).Task;
                if (result.Equals(true))
                {
                    Utility.DeleteEmptyDirectories(Config.DeleteMode,
                        Services.Provider.GetRequiredService<OfflineSyncConfig>().DeleteDirName);
                }
            }
        }
    }
}