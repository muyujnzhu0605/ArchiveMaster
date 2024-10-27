using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels.FileSystem
{
    public partial class LocalAndOffsiteDir : ObservableObject
    {
        [ObservableProperty]
        private string localDir;


        [ObservableProperty]
        private string offsiteDir;
    }
}