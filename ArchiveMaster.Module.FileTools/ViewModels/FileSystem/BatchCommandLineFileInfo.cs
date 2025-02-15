using ArchiveMaster.Configs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels.FileSystem;

public partial class BatchCommandLineFileInfo : SimpleFileInfo
{
    [ObservableProperty]
    private string commandLine;

    public BatchCommandLineFileInfo(FileSystemInfo file, string topDir, string commandLine) : base(file, topDir)
    {
        CommandLine = ReplaceFilePlaceholder(commandLine, file.FullName);
    }

    private static string ReplaceFilePlaceholder(string commandTemplate, string fileName)
    {
        if (!commandTemplate.Contains(BatchCommandLineConfig.PathPlaceholder))
        {
            throw new Exception($"命令行（{commandTemplate}）中不含占位符（{BatchCommandLineConfig.PathPlaceholder}）");
        }

        string escapedFileName;

        if (OperatingSystem.IsWindows())
        {
            // Windows 平台：双引号转义为两个双引号，并用双引号包裹
            escapedFileName = "\"" + fileName.Replace("\"", "\"\"") + "\"";
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            // Unix 平台：双引号转义为反斜杠加双引号，并用双引号包裹
            escapedFileName = "\"" + fileName.Replace("\"", "\\\"") + "\"";
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        // 替换命令行模板中的 {file} 占位符
        return commandTemplate.Replace(BatchCommandLineConfig.PathPlaceholder, escapedFileName);
    }
}