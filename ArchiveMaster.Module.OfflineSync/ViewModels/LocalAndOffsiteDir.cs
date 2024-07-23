using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using System.ComponentModel;

namespace ArchiveMaster.Model
{
    public partial class LocalAndOffsiteDir : ObservableObject
    {
        [ObservableProperty]
        private string localDir;


        [ObservableProperty]
        private string offsiteDir;
    }
}