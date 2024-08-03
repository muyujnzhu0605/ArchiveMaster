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
        public PackingPanelViewModel ViewModel { get; } = new PackingPanelViewModel();
    }
}
