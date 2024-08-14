using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ArchiveMaster.ViewModels
{
    public partial class EncryptorFileInfo : SimpleFileInfo
    {
        [ObservableProperty]
        private bool isEncrypted;

        [ObservableProperty]
        private bool isFileNameEncrypted;

        [ObservableProperty]
        private string relativePath;

        [ObservableProperty]
        private string targetName;

        [ObservableProperty]
        private string targetPath;

        [ObservableProperty]
        private string targetRelativePath;


        public EncryptorFileInfo(FileInfo file) : base(file)
        {
        }
    }
}
