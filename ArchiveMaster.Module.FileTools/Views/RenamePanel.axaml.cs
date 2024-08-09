using Avalonia.Controls;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    public partial class RenamePanel : TwoStepPanelBase
    {
        public RenamePanel()
        {
            DataContext = new RenameViewModel();
            InitializeComponent();
        }
    }
}
