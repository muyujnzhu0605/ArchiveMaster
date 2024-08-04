using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public partial class Step3Config : ConfigBase
    {
        [ObservableProperty]
        private DeleteMode deleteMode = DeleteMode.MoveToDeletedFolder;

        [ObservableProperty]
        private string patchDir;
    }
}