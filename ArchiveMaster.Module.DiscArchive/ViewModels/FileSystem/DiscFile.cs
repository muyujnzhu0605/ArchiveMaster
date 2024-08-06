using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels
{
    public partial class DiscFile : FileInfoWithStatus
    {
        public DiscFile() : base()
        {
            
        }
        public DiscFile(FileInfo file) : base(file)
        {
        }
        [ObservableProperty]
        private string discName;

        [ObservableProperty]
        private string md5;
    }
}