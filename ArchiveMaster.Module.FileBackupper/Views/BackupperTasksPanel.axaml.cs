using System.Diagnostics;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    /// <summary>
    /// RebuildPanel.xaml 的交互逻辑
    /// </summary>
    public partial class BackupperTasksPanel : TwoStepPanelBase
    {
        public BackupperTasksPanel(BackupperTasksViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}