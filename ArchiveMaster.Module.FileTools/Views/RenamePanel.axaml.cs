using Avalonia.Controls;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    public partial class RenamePanel : TwoStepPanelBase
    {
        public RenamePanel(RenameViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
