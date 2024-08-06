using DiscArchivingTool;

namespace ArchiveMaster.Views
{
    public partial class PackingPanel : TwoStepPanelBase
    {
        public PackingPanel()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }
        public PackingViewModel ViewModel { get; } = new PackingViewModel();
    }
}
