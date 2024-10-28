using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels.FileSystem
{
    public partial class DiscFilePackage : ObservableObject
    {
        [ObservableProperty]
        private int index;

        [ObservableProperty]
        private List<FileSystem.DiscFile> files = new List<FileSystem.DiscFile>();

        [ObservableProperty]
        private long totalLength;

        [ObservableProperty]
        private DateTime earliestTime;

        [ObservableProperty]
        private DateTime latestTime;

        [ObservableProperty]
        private bool isChecked;
    }
}