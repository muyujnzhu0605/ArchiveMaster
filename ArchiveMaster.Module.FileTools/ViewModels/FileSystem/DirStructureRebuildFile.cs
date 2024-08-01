using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels.FileSystem
{
    public partial class DirStructureRebuildFile : FileInfoWithStatus
    {
        public DirStructureRebuildFile()
        {
        }

        [ObservableProperty] 
        private bool multipleMatches;

        [ObservableProperty] 
        private bool rightPosition;
        
        [ObservableProperty]
        private SimpleFileInfo template;
    }
}
