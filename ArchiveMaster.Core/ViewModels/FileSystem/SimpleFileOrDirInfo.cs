using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArchiveMaster.ViewModels
{
    public partial class SimpleFileOrDirInfo : ObservableObject
    {
        public SimpleFileOrDirInfo()
        {
        }

        public SimpleFileOrDirInfo(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            Name = System.IO.Path.GetFileName(path);
            Path = path;
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string path;
    }
}
