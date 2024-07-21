using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArchiveMaster.ViewModels
{
    public partial class EncryptorFileInfo : SimpleFileInfo
    {
        [ObservableProperty]
        private bool isEnable = true;

        [ObservableProperty]
        private bool isEncrypted;

        [ObservableProperty]
        private bool isFileNameEncrypted;

        [ObservableProperty]
        private Exception error;

        [ObservableProperty]
        private string relativePath;

        [ObservableProperty]
        private string targetName;

        [ObservableProperty]
        private string targetPath;

        [ObservableProperty]
        private string targetRelativePath;


        public EncryptorFileInfo(string path) : base(path)
        {
        }
    }
}
