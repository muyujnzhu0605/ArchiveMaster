using Avalonia.Controls;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    public partial class UselessJpgCleanerPanel : TwoStepPanelBase
    {
        public UselessJpgCleanerPanel()
        {
            DataContext = new UselessJpgCleanerViewModel();
            InitializeComponent();
        }
    }
}
