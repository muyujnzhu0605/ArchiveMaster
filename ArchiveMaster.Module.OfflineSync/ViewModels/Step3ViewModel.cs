using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using ArchiveMaster.Views;
using System.Collections;
using ArchiveMaster.Enums;

namespace ArchiveMaster.ViewModels
{
    public partial class Step3ViewModel : OfflineSyncViewModelBase<SyncFileInfo>
    {
        [ObservableProperty]
        private DeleteMode deleteMode = DeleteMode.MoveToDeletedFolder;

        [ObservableProperty]
        private string patchDir;

        public IEnumerable DeleteModes => Enum.GetValues<DeleteMode>();
    }
}