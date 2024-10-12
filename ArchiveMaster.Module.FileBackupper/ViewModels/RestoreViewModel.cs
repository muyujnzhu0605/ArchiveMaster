using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Models;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ArchiveMaster.ViewModels.FileSystem;
using ArchiveMaster.Views;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;

namespace ArchiveMaster.ViewModels;

public partial class RestoreViewModel : TwoStepViewModelBase<RestoreUtility, BackupTask>
{
    [ObservableProperty]
    private bool isSnapshotComboBoxEnable;

    [ObservableProperty]
    private SimpleFileInfo selectedFile;

    [ObservableProperty]
    private BackupSnapshotEntity selectedSnapshot;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Config))]
    private BackupTask selectedTask;

    [ObservableProperty]
    private ObservableCollection<BackupSnapshotEntity> snapshots;

    [ObservableProperty]
    private ObservableCollection<BackupTask> tasks;

    [ObservableProperty]
    private BulkObservableCollection<SimpleFileInfo> treeFiles;
    public RestoreViewModel(AppConfig appConfig) : base(null, appConfig)
    {
    }

    public override BackupTask Config => SelectedTask;

    public override void OnEnter()
    {
        Tasks = new ObservableCollection<BackupTask>(Services.Provider.GetRequiredService<FileBackupperConfig>().Tasks);
        if (SelectedTask == null && Tasks.Count > 0)
        {
            SelectedTask = Tasks[0];
        }
    }

    protected override RestoreUtility CreateUtilityImplement()
    {
        return new RestoreUtility(SelectedTask, appConfig);
    }

    protected override Task OnInitializedAsync()
    {
        Utility.RootDir.Reorder();
        var files = new BulkObservableCollection<SimpleFileInfo>();
        files.AddRange(Utility.RootDir.Subs);
        TreeFiles = files;
        return base.OnInitializedAsync();
    }

    protected override Task OnInitializingAsync()
    {
        Utility.SnapShotId = SelectedSnapshot?.Id ?? throw new ArgumentNullException("未选择快照");
        return Task.CompletedTask;
    }

    async partial void OnSelectedTaskChanged(BackupTask value)
    {
        IsSnapshotComboBoxEnable = false;
        var utility = CreateUtilityImplement();
        Snapshots = new ObservableCollection<BackupSnapshotEntity>(await utility.GetSnapshotsAsync(default));
        if (Snapshots.Count > 0)
        {
            SelectedSnapshot = Snapshots[^1];
        }

        IsSnapshotComboBoxEnable = true;
    }

    [RelayCommand]
    private async Task SaveAsAsync()
    {
        if (SelectedFile is BackupFile file)
        {
            if (file.RecordEntity.PhysicalFile == null)
            {
                await this.ShowErrorAsync("备份文件不存在", "该文件不存在实际备份文件，可能是由虚拟快照生成");
                return;
            }

            var extension = Path.GetExtension(file.Name).TrimStart('.');
            var saveFile = await this.SendMessage(new GetStorageProviderMessage()).StorageProvider.SaveFilePickerAsync(
                new FilePickerSaveOptions()
                {
                    DefaultExtension = extension,
                    SuggestedFileName = file.Name,
                    FileTypeChoices =
                    [
                        new FilePickerFileType($"{extension}文件")
                            { Patterns = [$"*.{(extension.Length == 0 ? "*" : extension)}"] }
                    ]
                });
            var path = saveFile?.TryGetLocalPath();
            if (path != null)
            {
                var dialog = new FileProgressDialog();
                this.SendMessage(new DialogHostMessage(dialog));
                string backupFile = Path.Combine(SelectedTask.BackupDir, file.RecordEntity.PhysicalFile.FileName);
                if (!File.Exists(backupFile))
                {
                    await this.ShowErrorAsync("备份文件不存在", "该文件不存在实际备份文件，可能是文件丢失");
                    return;
                }

                await dialog.CopyFileAsync(backupFile, path);
            }
        }
    }
}