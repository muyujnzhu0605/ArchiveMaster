using System.ComponentModel;

namespace ArchiveMaster.Enums;

public enum SearchMode
{
    [Description("包含")]
    Contain,
    
    [Description("匹配扩展名")]
    EqualWithExtension,
    
    [Description("匹配文件名")]
    EqualWithName,
    
    [Description("匹配全名")]
    Equal,
    
    [Description("正则表达式")]
    Regex
}