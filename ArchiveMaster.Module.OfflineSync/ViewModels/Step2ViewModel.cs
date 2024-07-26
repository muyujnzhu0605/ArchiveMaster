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
        protected override Step2Config Config => AppConfig.Instance.Get<OfflineSyncConfig>().CurrentConfig.Step2;
        protected override Step2Utility Utility { get; } = new Step2Utility();

        [ObservableProperty] private string localDir;

        [ObservableProperty] private ObservableCollection<LocalAndOffsiteDir> matchingDirs;

        [ObservableProperty] private string offsiteSnapshot;

        partial void OnOffsiteSnapshotChanged(string value)
        {
            MatchingDirs = null;
        }

        [ObservableProperty] private string patchDir;

        public IEnumerable ExportModes => Enum.GetValues<ExportMode>();

        [ObservableProperty] public ExportMode exportMode;

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
                    new FilePickerFileType("异地备份快照") { Patterns = ["*.obos1"] }
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
                await this.SendMessage(new CommonDialogMessage()
                {
                    Type = CommonDialogMessage.CommonDialogType.Error,
                    Title = "导出失败",
                    Message = "本地和异地没有差异"
                }).Task;
                return;
            }

            if (string.IsNullOrWhiteSpace(PatchDir))
            {
                await this.SendMessage(new CommonDialogMessage()
                {
                    Type = CommonDialogMessage.CommonDialogType.Error,
                    Title = "导出失败",
                    Message = "未设置导出补丁目录"
                }).Task;
                return;
            }

            try
            {
                UpdateStatus(StatusType.Processing);
                bool allOk = true;
                await Task.Run(() => { allOk = Utility.Export(PatchDir, ExportMode); });
                if (!allOk)
                {
                    await this.SendMessage(new CommonDialogMessage()
                    {
                        Type = CommonDialogMessage.CommonDialogType.Error,
                        Title = "导出失败",
                        Message = "导出完成，但部分文件出现错误"
                    }).Task;
                    return;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await this.SendMessage(new CommonDialogMessage()
                {
                    Type = CommonDialogMessage.CommonDialogType.Error,
                    Title = "导出失败",
                    Exception = ex
                }).Task;
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
                await this.SendMessage(new CommonDialogMessage()
                {
                    Type = CommonDialogMessage.CommonDialogType.Error,
                    Title = "匹配失败",
                    Message = "未设置快照文件"
                }).Task;
                return;
            }

            if (!File.Exists(OffsiteSnapshot))
            {
                await this.SendMessage(new CommonDialogMessage()
                {
                    Type = CommonDialogMessage.CommonDialogType.Error,
                    Title = "匹配失败",
                    Message = "快照文件不存在"
                }).Task;
                return;
            }

            if (string.IsNullOrEmpty(LocalDir))
            {
                await this.SendMessage(new CommonDialogMessage()
                {
                    Type = CommonDialogMessage.CommonDialogType.Error,
                    Title = "匹配失败",
                    Message = "未设置本地目录"
                }).Task;
                return;
            }

            try
            {
                UpdateStatus(StatusType.Analyzing);
                await Task.Run(() =>
                {
                    string[] localSearchingDirs = LocalDir.Split(new char[] { '|', '\r', '\n' },
                        StringSplitOptions.RemoveEmptyEntries);
                    var step1 = Step1Utility.ReadStep1Model(OffsiteSnapshot);
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
                await this.SendMessage(new CommonDialogMessage()
                {
                    Type = CommonDialogMessage.CommonDialogType.Error,
                    Title = "匹配失败",
                    Exception = ex
                }).Task;
                return;
            }
            finally
            {
                UpdateStatus(StatusType.Ready);
            }
        }
    }
}