using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class DirStructureCloneConfig:ConfigBase
{
    [ObservableProperty]
    private string sourceDir;

    [ObservableProperty]
    private string targetDir;
}