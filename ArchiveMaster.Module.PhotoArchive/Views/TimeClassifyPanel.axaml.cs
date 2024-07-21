using Avalonia.Controls;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    public partial class TimeClassifyPanel : TwoStepPanelBase
    {
        public TimeClassifyPanel()
        {
            DataContext = new TimeClassifyViewModel();
            InitializeComponent();
        }
    }
}
