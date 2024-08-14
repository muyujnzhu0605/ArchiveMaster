using ArchiveMaster.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Utilities;

    public partial class RenameFileInfo : SimpleFileInfo
    {
        public RenameFileInfo(FileSystemInfo fileOrDir) : base(fileOrDir)
        {
        }
        
        public RenameFileInfo():base()
        {
        }

        [ObservableProperty]
        private bool isMatched;
        
        [ObservableProperty]
        private string newName;

        [ObservableProperty]
        private string newPath;
        
        [ObservableProperty]
        private string tempPath;
    }