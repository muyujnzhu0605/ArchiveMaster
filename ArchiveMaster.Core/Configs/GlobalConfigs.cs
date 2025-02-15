using ArchiveMaster.Enums;

namespace ArchiveMaster.Configs;

public class GlobalConfigs
{
    public static GlobalConfigs Instance { get; internal set; }
    public FilenameCasePolicy FileNameCase { get; set; } = FilenameCasePolicy.Auto;
}