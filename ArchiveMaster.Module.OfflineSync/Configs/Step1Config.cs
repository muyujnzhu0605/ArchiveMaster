using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public partial class Step1Config : ConfigBase
    {
        [ObservableProperty]
        private string outputFile;

        [ObservableProperty]
        private ObservableCollection<string> syncDirs;
    }
}