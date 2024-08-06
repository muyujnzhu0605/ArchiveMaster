using FzLib;
using System.ComponentModel;
using ArchiveMaster.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels
{
    public partial class DiscFilePackage : ObservableObject
    {
        [ObservableProperty]
        private int index;

        [ObservableProperty]
        private List<DiscFile> files = new List<DiscFile>();

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