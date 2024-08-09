using System.ComponentModel;

namespace ArchiveMaster.Enums;

public enum RenameMode
{
    [Description("替换关键词")]
    ReplaceMatched,
    
    [Description("替换扩展名")]
    ReplaceExtension,
    
    [Description("替换文件名")]
    ReplaceName,
    
    [Description("替换全名")]
    ReplaceAll,
}