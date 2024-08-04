using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ArchiveMaster.Views
{
    /// <summary>
    /// UpdatePanel.xaml 的交互逻辑
    /// </summary>
    public partial class Step1Panel : TwoStepPanelBase
    {
        public Step1Panel()
        {
            DataContext =  new Step1ViewModel();
            InitializeComponent();
        }

    }
}