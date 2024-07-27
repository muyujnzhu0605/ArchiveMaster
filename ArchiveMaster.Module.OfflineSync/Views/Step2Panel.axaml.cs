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

        

    }
}