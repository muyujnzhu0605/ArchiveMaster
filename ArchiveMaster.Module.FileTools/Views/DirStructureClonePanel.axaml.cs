using System.Collections.ObjectModel;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    /// <summary>
    /// RebuildPanel.xaml 的交互逻辑
    /// </summary>
    public partial class DirStructureClonePanel : TwoStepPanelBase
    {
        public DirStructureClonePanel( )
        {
            ViewModel = new DirStructureCloneViewModel();
            DataContext = ViewModel;
            InitializeComponent();
        }

        public DirStructureCloneViewModel ViewModel { get; }
    }

 
}