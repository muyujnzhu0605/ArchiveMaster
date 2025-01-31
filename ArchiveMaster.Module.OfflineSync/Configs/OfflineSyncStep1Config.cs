using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public partial class OfflineSyncStep1Config : ConfigBase
    {
        [ObservableProperty]
        private string outputFile;

        [ObservableProperty]
        private ObservableCollection<string> syncDirs;

        public override void Check()
        {
            CheckEmpty(OutputFile,"快照文件");
            foreach (var dir in SyncDirs)
            {
                CheckDir(dir,$"目录{dir}");
            }
        }
    }
}