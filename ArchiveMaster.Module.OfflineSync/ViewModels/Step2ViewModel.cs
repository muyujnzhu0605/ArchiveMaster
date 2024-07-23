using ArchiveMaster.Configs;
using ArchiveMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using Newtonsoft.Json;
using ArchiveMaster.Model;
using ArchiveMaster.Views;
using System.Collections;
using System.Collections.ObjectModel;

namespace ArchiveMaster.ViewModels
{
    public partial class Step2ViewModel : OfflineSyncViewModelBase<SyncFileInfo>
    {
        [ObservableProperty]
        private string blackList = "Thumbs.db";

        [ObservableProperty]
        private bool blackListUseRegex;
        
        [ObservableProperty]
        private ExportMode exportMode = ExportMode.Copy;

        [ObservableProperty]
        private string localDir;

        [ObservableProperty]
        private ObservableCollection<LocalAndOffsiteDir> matchingDirs;


        [ObservableProperty]
        private bool moveFileIgnoreName = true;


        [ObservableProperty]
        private string offsiteSnapshot;

        partial void OnOffsiteSnapshotChanged(string value)
        {
            MatchingDirs = null;
        }

        [ObservableProperty]
        private string patchDir;

        public IEnumerable ExportModes => Enum.GetValues<ExportMode>();
    }
}