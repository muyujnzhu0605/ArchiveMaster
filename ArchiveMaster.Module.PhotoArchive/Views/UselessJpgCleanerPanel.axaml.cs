using Avalonia.Controls;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    public partial class UselessJpgCleanerPanel : TwoStepPanelBase
    {
        public UselessJpgCleanerPanel(UselessJpgCleanerViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
