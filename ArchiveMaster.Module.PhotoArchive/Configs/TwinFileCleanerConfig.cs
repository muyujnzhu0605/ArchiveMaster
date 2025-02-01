using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public partial class TwinFileCleanerConfig : ConfigBase
    {
        [ObservableProperty]
        private string dir;

        [ObservableProperty]
        private string searchExtension = "DNG";
        
        [ObservableProperty]
        private string deletingExtension = "JPG";
        
        public override void Check()
        {
            CheckDir(Dir,"目录");
            CheckEmpty(SearchExtension,"搜索后缀名");
            CheckEmpty(DeletingExtension,"待删除后缀名");
        }
    }
}
