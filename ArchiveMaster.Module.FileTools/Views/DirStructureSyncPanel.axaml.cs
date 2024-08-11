using System.Collections.ObjectModel;

namespace ArchiveMaster.Views
{
    /// <summary>
    /// RebuildPanel.xaml 的交互逻辑
    /// </summary>
    public partial class DirStructureSyncPanel : TwoStepPanelBase
    {

        public DirStructureSyncPanel( )
        {
            ViewModel = new ViewModels.DirStructureSyncViewModel();
            DataContext = ViewModel;
            InitializeComponent();

           
        }

        public ViewModels.DirStructureSyncViewModel ViewModel { get; }


    }
}