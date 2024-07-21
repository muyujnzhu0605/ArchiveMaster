using Avalonia.Controls;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    public partial class RepairModifiedTimePanel : TwoStepPanelBase
    {
        public RepairModifiedTimePanel()
        {
            DataContext = new RepairModifiedTimeViewModel();
            InitializeComponent();
        }
    }
}
