using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs
{
    public partial class Step3Config : ConfigBase
    {
        [ObservableProperty]
        private DeleteMode deleteMode = DeleteMode.MoveToDeletedFolder;

        [ObservableProperty]
        private string deleteDir = "异地备份离线同步-删除的文件";

        [ObservableProperty]
        private string patchDir;
    }
}