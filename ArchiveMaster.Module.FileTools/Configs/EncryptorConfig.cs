using System;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public partial class EncryptorConfig : ConfigBase
    {
        public enum EncryptorTaskType
        {
            Encrypt,
            Decrypt,
        }

        [ObservableProperty]
        private CipherMode cipherMode = CipherMode.CBC;

        [ObservableProperty]
        private bool deleteSourceFiles;

        [ObservableProperty]
        private string encryptedDir;

        [ObservableProperty]
        private bool encryptDirectoryStructure;

        [ObservableProperty]
        private bool encryptFileNames;

        [ObservableProperty]
        private bool encryptFolderNames;

        [ObservableProperty]
        private bool overwriteExistedFiles;

        [ObservableProperty]
        private PaddingMode paddingMode = PaddingMode.PKCS7;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string rawDir;

        [ObservableProperty]
        private bool rememberPassword;

        [ObservableProperty]
        private EncryptorTaskType type = EncryptorTaskType.Encrypt;

        public override void Check()
        {
            CheckDir(EncryptedDir,"加密目录");
            CheckDir(RawDir,"原始目录");
            CheckEmpty(Password,"密码");
        }
    }
}
