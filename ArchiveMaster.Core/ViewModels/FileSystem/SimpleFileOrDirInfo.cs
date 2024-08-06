using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArchiveMaster.ViewModels
{
    public partial class SimpleFileOrDirInfo : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string path;

        public SimpleFileOrDirInfo()
        {
        }

        public SimpleFileOrDirInfo(FileSystemInfo file)
        {
            ArgumentNullException.ThrowIfNull(file);
            Name = file.Name;
            Path = file.FullName;
        }
    }
}
