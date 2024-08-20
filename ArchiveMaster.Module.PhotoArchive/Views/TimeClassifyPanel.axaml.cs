using Avalonia.Controls;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    public partial class TimeClassifyPanel : TwoStepPanelBase
    {
        public TimeClassifyPanel(TimeClassifyViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}
