using System.Collections.ObjectModel;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    /// <summary>
    /// RebuildPanel.xaml 的交互逻辑
    /// </summary>
    public partial class DirStructureClonePanel : TwoStepPanelBase
    {
        public DirStructureClonePanel(DirStructureCloneViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}