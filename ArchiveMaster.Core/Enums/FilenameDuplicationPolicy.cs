using System.ComponentModel;

namespace ArchiveMaster.Enums;

public enum FilenameDuplicationPolicy
{
    [Description("跳过")]
    Skip,
    [Description("覆盖")]
    Overwrite,
    [Description("报错")]
    Throw
}