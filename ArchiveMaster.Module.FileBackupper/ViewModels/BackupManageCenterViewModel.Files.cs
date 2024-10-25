using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.ViewModels.FileSystem;
using ArchiveMaster.Views;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;

namespace ArchiveMaster.ViewModels;

public partial class BackupManageCenterViewModel
{
    [ObservableProperty]
    private SimpleFileInfo selectedFile;

    [ObservableProperty]
    private BulkObservableCollection<SimpleFileInfo> treeFiles;

    [RelayCommand]
    private async Task SaveAsAsync()
    {
        switch (SelectedFile)
        {
            case BackupFile file:
                await SaveFile(file);
                break;
            case TreeDirInfo dir:
                await SaveFolder(dir);
                break;
        }
    }

    private async Task SaveFile(BackupFile file)
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

            await dialog.CopyFileAsync(backupFile, path, file.Time);
        }
    }

    private async Task SaveFolder(TreeDirInfo dir)
    {
        var folders = await this.SendMessage(new GetStorageProviderMessage()).StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions());
        if (folders is { Count: 1 })
        {
            var rootDir = folders[0].TryGetLocalPath();
            var dialog = new FileProgressDialog();
            this.SendMessage(new DialogHostMessage(dialog));
            var files = dir.Flatten();
            List<string> sourcePaths = new List<string>();
            List<string> destinationPaths = new List<string>();
            List<DateTime> times = new List<DateTime>();
            foreach (var file in files.Cast<BackupFile>())
            {
                string backupFile = Path.Combine(SelectedTask.BackupDir, file.RecordEntity.PhysicalFile.FileName);
                string fileRelativePath = dir.RelativePath == null
                    ? file.RelativePath
                    : Path.GetRelativePath(dir.RelativePath, file.RelativePath);
                string destinationPath = Path.Combine(rootDir, fileRelativePath);
                sourcePaths.Add(backupFile);
                destinationPaths.Add(destinationPath);
                times.Add(file.Time);

                if (!File.Exists(backupFile))
                {
                    await this.ShowErrorAsync("备份文件不存在", "该文件不存在实际备份文件，可能是文件丢失");
                    return;
                }
            }

            await dialog.CopyFilesAsync(sourcePaths, destinationPaths, times);
        }
    }
}