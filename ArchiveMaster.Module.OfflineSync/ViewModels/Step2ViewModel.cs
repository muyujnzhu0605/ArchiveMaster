using ArchiveMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using Newtonsoft.Json;
using ArchiveMaster.Model;
using ArchiveMaster.Views;
using System.Collections;
using System.Collections.ObjectModel;
using ArchiveMaster.Enums;
using ArchiveMaster.Configs;
using ArchiveMaster.UI.ViewModels;
using ArchiveMaster.Utility;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;
using Mapster;

namespace ArchiveMaster.ViewModels
{
    public partial class Step2ViewModel : OfflineSyncViewModelBase<SyncFileInfo>
    {
        [ObservableProperty]
        private string blackList;

        [ObservableProperty]
        private bool blackListUseRegex;

        [ObservableProperty]
        private ExportMode exportMode;
        [ObservableProperty]
        private string localDir;

        [ObservableProperty]
        private ObservableCollection<LocalAndOffsiteDir> matchingDirs;

        [ObservableProperty]
        private string offsiteSnapshot;

        [ObservableProperty] 
        private string patchDir;
        Step1Model step1 = null;
        public IEnumerable ExportModes => Enum.GetValues<ExportMode>();
        protected override OfflineSyncStepConfigBase Config => AppConfig.Instance.Get<OfflineSyncConfig>().CurrentConfig.Step2;
        protected override Step2Utility Utility { get; } = new Step2Utility();

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
                if (string.IsNullOrEmpty(LocalDir))
                {
                    LocalDir = path;
                }
                else
                {
                    LocalDir += Environment.NewLine + path;
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
                OffsiteSnapshot = files[0].TryGetLocalPath();
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
                PatchDir = folders[0].TryGetLocalPath();
            }
        }

        [RelayCommand]
        private async Task ExportAsync()
        {
            if (Files.Count == 0)
            {
                await this.ShowErrorAsync("导出失败", "本地和异地没有差异");
                return;
            }

            if (string.IsNullOrWhiteSpace(PatchDir))
            {
                await this.ShowErrorAsync("导出失败", "未设置导出补丁目录");
                return;
            }

            try
            {
                UpdateStatus(StatusType.Processing);
                bool allOk = true;
                await Task.Run(() => { allOk = Utility.Export(PatchDir, ExportMode); });
                if (!allOk)
                {
                    await this.ShowErrorAsync("导出失败", "导出完成，但部分文件出现错误");
                    return;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync("导出失败", ex);
                return;
            }
            finally
            {
                UpdateStatus(StatusType.Analyzed);
            }
        }

        [RelayCommand]
        private async Task MatchDirsAsync()
        {
            if (string.IsNullOrEmpty(OffsiteSnapshot))
            {
                await this.ShowErrorAsync("匹配失败", "未设置快照文件");
                return;
            }

            if (!File.Exists(OffsiteSnapshot))
            {
                await this.ShowErrorAsync("匹配失败", "快照文件不存在");
                return;
            }

            if (string.IsNullOrEmpty(LocalDir))
            {
                await this.ShowErrorAsync("匹配失败", "未设置本地目录");
                return;
            }

            try
            {
                UpdateStatus(StatusType.Analyzing);
                await Task.Run(() =>
                {
                    string[] localSearchingDirs = LocalDir.Split(new char[] { '|', '\r', '\n' },
                        StringSplitOptions.RemoveEmptyEntries);
                    step1 = Step1Utility.ReadStep1Model(OffsiteSnapshot);
                    MatchingDirs =
                        new ObservableCollection<LocalAndOffsiteDir>(
                            Step2Utility.MatchLocalAndOffsiteDirs(step1, localSearchingDirs));
                });
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync("匹配失败", ex);
            }
            finally
            {
                UpdateStatus(StatusType.Ready);
            }
        }

        partial void OnOffsiteSnapshotChanged(string value)
        {
            MatchingDirs = null;
        }
        [RelayCommand]
        private async Task SearchChangeAsync()
        {
            if (step1 == null)
            {
                await this.ShowErrorAsync("查找失败", "请先匹配目录");
                return;
            }
            if (MatchingDirs is null or { Count: 0 })
            {
                await this.ShowErrorAsync("查找失败", "没有匹配的目录");
                return;
            }
            bool needProcess = false;
            try
            {
                UpdateStatus(StatusType.Analyzing);
                await Task.Run(() =>
                {
                    Utility.Search(MatchingDirs, step1, BlackList,
                        BlackListUseRegex, OfflineSyncConfig.MaxTimeTolerance,
                        false);
                    Files = new ObservableCollection<SyncFileInfo>(Utility.UpdateFiles);
                });
                if (Files.Count == 0)
                {
                    await this.ShowOkAsync("查找完成", "本地和异地没有差异");
                }
                else
                {
                    needProcess = true;
                }
                UpdateStatus(needProcess ? StatusType.Analyzed : StatusType.Ready);
            }
            catch (OperationCanceledException)
            {
                UpdateStatus(StatusType.Ready);
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync("查找失败",ex);
                UpdateStatus(StatusType.Ready);
            }
        }

    }
}