using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels
{
    public partial class DiscFile : FileInfoWithStatus
    {
        [ObservableProperty]
        private string discName;

        [ObservableProperty]
        private string md5;
    }
}