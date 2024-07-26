using System.Reflection;
using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Utility;

namespace ArchiveMaster.Views
{
    /// <summary>
    /// RebuildPanel.xaml 的交互逻辑
    /// </summary>
    public partial class Step2Panel : OfflineSyncPanelBase
    {
        public Step2Panel()
        {
            ViewModel = new Step2ViewModel();
            DataContext = ViewModel;
            ViewModel.Files.Add(new SyncFileInfo(new FileInfo(@"C:\Users\autod\Documents\Apps\自写软件\FrpGUI\config.json"),@"C:\Users\autod\"){UpdateType = FileUpdateType.Add});
            InitializeComponent();
        }

        public Step2ViewModel ViewModel { get; }

        

     
        //
        // private async void SearchChangeButton_Click(object sender, RoutedEventArgs e)
        // {
        //     if (step1 == null)
        //     {
        //         await CommonDialog.ShowErrorDialogAsync("请先匹配目录");
        //         return;
        //     }
        //     if (ViewModel.MatchingDirs is null or { Count: 0 })
        //     {
        //         await CommonDialog.ShowErrorDialogAsync("没有匹配的目录");
        //         return;
        //     }
        //     bool needProcess = false;
        //     try
        //     {
        //         ViewModel.UpdateStatus(StatusType.Analyzing);
        //         await Task.Run(() =>
        //         {
        //             u.Search(ViewModel.MatchingDirs, step1, ViewModel.BlackList,
        //                 ViewModel.BlackListUseRegex, OfflineSyncConfigs.MaxTimeTolerance,
        //                 ViewModel.MoveFileIgnoreName);
        //             ViewModel.Files = new ObservableCollection<SyncFileInfo>(u.UpdateFiles);
        //         });
        //         if (ViewModel.Files.Count == 0)
        //         {
        //             await CommonDialog.ShowOkDialogAsync("查找完成", "本地和异地没有差异");
        //         }
        //         else
        //         {
        //             needProcess = true;
        //         }
        //         ViewModel.UpdateStatus(needProcess ? StatusType.Analyzed : StatusType.Ready);
        //     }
        //     catch (OperationCanceledException)
        //     {
        //         ViewModel.UpdateStatus(StatusType.Ready);
        //     }
        //     catch (Exception ex)
        //     {
        //         await CommonDialog.ShowErrorDialogAsync(ex, "查找失败");
        //         ViewModel.UpdateStatus(StatusType.Ready);
        //     }
        // }
        //
        // private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        // {
        //     ViewModel.Files?.ForEach(p => p.Checked = true);
        // }
        //
        // private void SelectNoneButton_Click(object sender, RoutedEventArgs e)
        // {
        //     ViewModel.Files?.ForEach(p => p.Checked = false);
        // }
        //
        // private void StopButton_Click(object sender, RoutedEventArgs e)
        // {
        //     ViewModel.UpdateStatus(StatusType.Stopping);
        //     u.Stop();
        // }
    }
}