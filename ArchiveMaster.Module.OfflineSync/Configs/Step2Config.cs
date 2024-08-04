using ArchiveMaster.Model;
using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public partial class Step2Config : ConfigBase
    {
        [ObservableProperty]
        private string blackList;

        [ObservableProperty]
        private bool blackListUseRegex;

        [ObservableProperty]
        private ExportMode exportMode = ExportMode.Copy;

        [ObservableProperty]
        private string localDir;

        [ObservableProperty]
        private string patchDir;

        [ObservableProperty]
        private string offsiteSnapshot;

        [ObservableProperty]
        private List<LocalAndOffsiteDir> matchingDirs;
    }
}