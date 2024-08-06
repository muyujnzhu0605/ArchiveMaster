using ArchiveMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using ArchiveMaster.Model;
using ArchiveMaster.Views;
using System.Collections;
using System.Collections.ObjectModel;
using ArchiveMaster.Enums;
using ArchiveMaster.Configs;
using ArchiveMaster.UI.ViewModels;
using ArchiveMaster.Utilities;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;
using Mapster;

namespace ArchiveMaster.ViewModels
{
    public partial class Step2ViewModel : OfflineSyncViewModelBase<Step2Utility, SyncFileInfo>
    {
        public IEnumerable ExportModes => Enum.GetValues<ExportMode>();
        public override Step2Config Config => AppConfig.Instance.Get<OfflineSyncConfig>().CurrentConfig.Step2;

        [RelayCommand]
        private async Task BrowseLocalDirAsync()
        {
            var provider = this.SendMessage(new GetStorageProviderMessage()).StorageProvider;
            var folders = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                AllowMultiple = true,
            });

            if (folders.Count > 0)
            {
                string path = string.Join(Environment.NewLine, folders.Select((p => p.TryGetLocalPath())));
                if (string.IsNullOrEmpty(Config.LocalDir))
                {
                    Config.LocalDir = path;
                }
                else
                {
                    Config.LocalDir += Environment.NewLine + path;
                }
            }
        }

        [RelayCommand]
        private async Task BrowseOffsiteSnapshotAsync()
        {
            var provider = this.SendMessage(new GetStorageProviderMessage()).StorageProvider;
            var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                FileTypeFilter =
                [
                    new FilePickerFileType("异地备份快照") { Patterns = ["*.os1"] }
                ]
            });

            if (files.Count > 0)
            {
                Config.OffsiteSnapshot = files[0].TryGetLocalPath();
            }
        }

        [RelayCommand]
        private async Task BrowsePatchDirAsync()
        {
            var provider = this.SendMessage(new GetStorageProviderMessage()).StorageProvider;
            var folders = await provider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                AllowMultiple = false,
            });

            if (folders.Count > 0)
            {
                Config.PatchDir = folders[0].TryGetLocalPath();
            }
        }

        protected override Task OnExecutingAsync(CancellationToken token)
        {
            if (Files.Count == 0)
            {
                throw new Exception("本地和异地没有差异");
            }

            return base.OnExecutingAsync(token);
        }

        protected override async Task OnExecutedAsync(CancellationToken token)
        {
            if (Utility.HasError)
            {
                await this.ShowErrorAsync("导出失败", "导出完成，但部分文件出现错误");
            }
        }

        [RelayCommand]
        private async Task MatchDirsAsync()
        {
            try
            {
               Config.Check();

                string[] localSearchingDirs = Config.LocalDir.Split(new[] { '|', '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);
                Config.MatchingDirs =
                    new ObservableCollection<LocalAndOffsiteDir>(await Step2Utility
                        .MatchLocalAndOffsiteDirsAsync(Config.OffsiteSnapshot, localSearchingDirs));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync("匹配失败", ex);
            }
        }

        protected override Task OnInitializingAsync()
        {
            if (Config.MatchingDirs is null or { Count: 0 })
            {
                throw new Exception("没有匹配的目录");
            }

            return base.OnInitializingAsync();
        }

        protected override Task OnInitializedAsync()
        {
            Files = new ObservableCollection<SyncFileInfo>(Utility.UpdateFiles);
            return base.OnInitializedAsync();
        }
    }
}