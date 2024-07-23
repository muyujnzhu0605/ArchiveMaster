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

        protected FileInfo FileInfo { get;private set; }
        public SimpleFileOrDirInfo()
        {
        }

        public SimpleFileOrDirInfo(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            FileInfo = new FileInfo(path);
            Name = FileInfo.Name;
            Path = FileInfo.FullName;
        }
    }
}
