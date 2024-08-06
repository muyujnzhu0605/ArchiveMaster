using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
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
        [property:JsonIgnore]
        private ObservableCollection<LocalAndOffsiteDir> matchingDirs;

        partial void OnOffsiteSnapshotChanged(string value)
        {
            MatchingDirs = null;
        }

        public override void Check()
        {
            CheckFile(OffsiteSnapshot,"异地快照文件");
            CheckEmpty(LocalDir,"本地目录");
        }
    }
}