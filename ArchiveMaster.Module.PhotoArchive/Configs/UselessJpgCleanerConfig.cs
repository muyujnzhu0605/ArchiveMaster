using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public partial class UselessJpgCleanerConfig : ConfigBase
    {
        [ObservableProperty]
        private string dir;

        [ObservableProperty]
        private string rawExtension = "DNG";
        
        public override void Check()
        {
            CheckDir(Dir,"目录");
            CheckEmpty(RawExtension,"RAW后缀名");
        }
    }
}
