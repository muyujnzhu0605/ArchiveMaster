using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class DirStructureCloneConfig:ConfigBase
{
    [ObservableProperty]
    private string sourceDir;

    [ObservableProperty]
    private string targetDir;

    public override void Check()
    {
        CheckDir(SourceDir,"源目录");
        CheckEmpty(SourceDir,"目标目录");
    }
}