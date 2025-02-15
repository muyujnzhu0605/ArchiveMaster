using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using LocalAndOffsiteDir = ArchiveMaster.ViewModels.FileSystem.LocalAndOffsiteDir;

namespace ArchiveMaster.Configs
{
    public partial class OfflineSyncStep2Config : ConfigBase
    {
        [ObservableProperty]
        private FileFilterConfig filter = new FileFilterConfig();

        [ObservableProperty]
        private ExportMode exportMode = ExportMode.Copy;

        [ObservableProperty]
        private bool checkMoveIgnoreFileName = true;

        [ObservableProperty]
        private string localDir;

        [ObservableProperty]
        private int maxTimeToleranceSecond = 3;

        [ObservableProperty]
        private string patchDir;

        [ObservableProperty]
        private string offsiteSnapshot;

        [ObservableProperty]
        [property: JsonIgnore]
        private ObservableCollection<LocalAndOffsiteDir> matchingDirs;

        partial void OnOffsiteSnapshotChanged(string value)
        {
            MatchingDirs = null;
        }

        public override void Check()
        {
            CheckFile(OffsiteSnapshot, "异地快照文件");
            CheckEmpty(LocalDir, "本地搜索目录");
        }
    }
}