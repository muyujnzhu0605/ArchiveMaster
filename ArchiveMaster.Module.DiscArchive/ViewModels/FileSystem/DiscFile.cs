using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels.FileSystem
{
    public partial class DiscFile : SimpleFileInfo
    {
        public DiscFile()
        {
            
        }
        public DiscFile(FileInfo file,string topDir) : base(file,topDir)
        {
        }
        [ObservableProperty]
        private string discName;

        [ObservableProperty]
        private string md5;
    }
}