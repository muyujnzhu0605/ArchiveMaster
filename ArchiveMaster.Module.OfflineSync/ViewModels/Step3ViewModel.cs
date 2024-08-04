using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using ArchiveMaster.Views;
using System.Collections;
using System.Collections.ObjectModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.UI.ViewModels;
using ArchiveMaster.Utilities;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;

namespace ArchiveMaster.ViewModels
{
    public partial class Step3ViewModel : OfflineSyncViewModelBase<SyncFileInfo>
    {
        [ObservableProperty] private DeleteMode deleteMode = DeleteMode.MoveToDeletedFolder;

        [ObservableProperty] private string patchDir;
        public IEnumerable DeleteModes => Enum.GetValues<DeleteMode>();

        protected override ConfigBase Config =>
            AppConfig.Instance.Get<OfflineSyncConfig>().CurrentConfig.Step3;

        protected override Step3Utility Utility { get; } = new Step3Utility();

        [RelayCommand]
        private async Task AnalyzeAsync()
        {
            if (string.IsNullOrEmpty(PatchDir))
            {
                await this.ShowErrorAsync("分析失败", "未设置补丁目录");
                return;
            }

            if (!Directory.Exists(PatchDir))
            {
                await this.ShowErrorAsync("分析失败", "补丁目录不存在");
                return;
            }

            try
            {
                UpdateStatus(StatusType.Analyzing);
                await Task.Run(() =>
                {
                    Utility.Analyze(PatchDir);
                    Files = new ObservableCollection<SyncFileInfo>(Utility.UpdateFiles);
                });
                UpdateStatus(Files.Count > 0 ? StatusType.Analyzed : StatusType.Ready);
            }
            catch (OperationCanceledException)
            {
                UpdateStatus(StatusType.Ready);
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync("分析失败", ex);
                UpdateStatus(StatusType.Ready);
            }
        }

        [RelayCommand]
        private async Task UpdateAsync()
        {
            try
            {
                UpdateStatus(StatusType.Processing);
                await Task.Run(() =>
                {
                    Utility.Update(DeleteMode, AppConfig.Instance.Get<OfflineSyncConfig>().DeleteDirName);
                    Utility.AnalyzeEmptyDirectories();
                });

                if (Utility.DeletingDirectories.Count != 0)
                {
                    var result = await this.SendMessage(new CommonDialogMessage()
                    {
                        Title = "删除空目录",
                        Message = $"有{Utility.DeletingDirectories.Count}个已不存在于本地的空目录，是否删除？",
                        Detail = string.Join(Environment.NewLine,
                            Utility.DeletingDirectories.Select(p => Path.Combine(p.TopDirectory, p.Path))),
                        Type = CommonDialogMessage.CommonDialogType.YesNo
                    }).Task;
                    if (result.Equals(true))
                    {
                        Utility.DeleteEmptyDirectories(DeleteMode,
                            AppConfig.Instance.Get<OfflineSyncConfig>().DeleteDirName);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await this.ShowErrorAsync("更新失败", ex);
            }
            finally
            {
                UpdateStatus(StatusType.Analyzed);
            }
        }
    }
}