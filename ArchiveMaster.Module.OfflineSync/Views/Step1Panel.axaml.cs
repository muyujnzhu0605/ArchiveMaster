using ArchiveMaster.Configs;
using ArchiveMaster.Utility;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ArchiveMaster.Views
{
    /// <summary>
    /// UpdatePanel.xaml 的交互逻辑
    /// </summary>
    public partial class Step1Panel : UserControl
    {
        private readonly Step1Utility u = new Step1Utility();

        public Step1Panel()
        {
            ViewModel = AppConfig.Instance.Get<OfflineSyncConfig>().CurrentConfig.Step1;
            DataContext = ViewModel;
            InitializeComponent();
            PanelHelper.RegisterMessageAndProgressEvent(u, ViewModel);
        }


        public Step1ViewModel ViewModel { get; }

        private async void BrowseDirButton_Click(object sender, RoutedEventArgs e)
        {
            //var dialog = new OpenFolderDialog();
            //dialog.Multiselect = true;
            //if (dialog.ShowDialog() == true)
            //{
            //    var paths = dialog.FolderNames;
            //    if (paths.Length > 0)
            //    {
            //        foreach (var path in paths)
            //        {
            //            try
            //            {
            //                ViewModel.AddSyncDir(path);
            //            }
            //            catch (Exception ex)
            //            {
            //                await CommonDialog.ShowErrorDialogAsync(ex.Message, null, "添加失败");
            //            }
            //        }
            //    }
            //}
        }


        private void BrowseOutputFileButton_Click(object sender, RoutedEventArgs e)
        {
            GetObos1File();
        }

        private bool GetObos1File()
        {
            return false;
            //string name = $"{DateTime.Now:yyyyMMdd}-备份";
            //var dialog = new SaveFileDialog().AddFilter("异地备份快照", "obos1");
            //dialog.FileName = name;
            //string path = dialog.GetPath(this.GetWindow());
            //if (path != null)
            //{
            //    ViewModel.OutputFile = path;
            //    return true;
            //}
            //return false;
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            //var dirs = ViewModel.SyncDirs.ToHashSet();
            //if (dirs.Count == 0)
            //{
            //    await CommonDialog.ShowErrorDialogAsync("选择的目录为空");
            //    return;
            //}
            //foreach (var dir1 in dirs)
            //{
            //    foreach (var dir2 in dirs.Where(p => p != dir1))
            //    {
            //        if (dir1.StartsWith(dir2))
            //        {
            //            await CommonDialog.ShowErrorDialogAsync($"目录存在嵌套：{dir1}是{dir2}的子目录");
            //            return;
            //        }
            //    }
            //}
            //if (string.IsNullOrWhiteSpace(ViewModel.OutputFile))
            //{
            //    if (!GetObos1File())
            //    {
            //        return;
            //    }
            //}


            //ViewModel.UpdateStatus(StatusType.Processing);
            //try
            //{
            //    await Task.Run(() =>
            //    {
            //        u.Enumerate(dirs, ViewModel.OutputFile);
            //    });
            //}
            //catch (OperationCanceledException)
            //{

            //}
            //catch (Exception ex)
            //{
            //    await CommonDialog.ShowErrorDialogAsync(ex, "导出失败");
            //}
            //finally
            //{
            //    ViewModel.UpdateStatus(StatusType.Ready);
            //}

        }

        private void RemoveAllSyncDirsButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SyncDirs.Clear();
        }

        private void RemoveSelectedSyncDirsButton_Click(object sender, RoutedEventArgs e)
        {
            //string dir = lvwSelectedDirs.SelectedItem as string;
            //ViewModel.SyncDirs.Remove(dir);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateStatus(StatusType.Stopping);
            u.Stop();
        }

        private async void InputDirButton_Click(object sender, RoutedEventArgs e)
        {
            //var paths = await CommonDialog.ShowInputDialogAsync("请输入目录，一行一个", multiLines: true, maxLines: int.MaxValue);
            //if (paths != null)
            //{
            //    foreach (var path in paths.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            //    {
            //        if (string.IsNullOrWhiteSpace(path))
            //        {
            //            continue;
            //        }
            //        if (!Directory.Exists(path))
            //        {
            //            await CommonDialog.ShowErrorDialogAsync($"目录{path}不存在");
            //            continue;
            //        }
            //        try
            //        {
            //            ViewModel.AddSyncDir(path);
            //        }
            //        catch (Exception ex)
            //        {
            //            await CommonDialog.ShowErrorDialogAsync(ex.Message,null, "添加失败");
            //        }
            //    }
            //}
        }
    }
}