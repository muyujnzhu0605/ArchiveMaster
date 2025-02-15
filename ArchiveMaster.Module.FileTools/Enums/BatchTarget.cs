using System.ComponentModel;

namespace ArchiveMaster.Enums;

public enum BatchTarget
{
    [Description("每个文件")]
    EachFiles,

    [Description("每个目录")]
    EachDirs,

    [Description("每个文件和目录")]
    EachElement,

    [Description("顶层文件")]
    TopFiles,

    [Description("顶层目录")]
    TopDirs,

    [Description("顶层文件和目录")]
    TopElements,

    [Description("指定深度文件")]
    SpecialLevelDirs,

    [Description("指定深度目录")]
    SpecialLevelFiles,

    [Description("指定深度文件和目录")]
    SpecialLevelElements
}