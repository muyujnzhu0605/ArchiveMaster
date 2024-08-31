using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class DirStructureCloneConfig : ConfigBase
{
    [ObservableProperty]
    private string sourceDir;

    [ObservableProperty]
    private string targetDir;

    [ObservableProperty]
    private string targetFile;

    public override void Check()
    {
        CheckDir(SourceDir, "源目录");
        if (!string.IsNullOrWhiteSpace(TargetDir) && !OperatingSystem.IsWindows())
        {
            throw new Exception("稀疏文件目前仅支持Windows");
        }

        if (string.IsNullOrWhiteSpace(TargetDir) && string.IsNullOrWhiteSpace(TargetFile))
        {
            throw new Exception($"稀疏文件目录或结构文件至少填写一项");
        }
    }
}