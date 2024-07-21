using Avalonia.Controls;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Views
{
    public partial class EncryptorPanel : TwoStepPanelBase
    {
        public EncryptorPanel()
        {
            DataContext = new EncryptorViewModel();
            InitializeComponent();
        }
    }
}
