using System.ComponentModel;

namespace ArchiveMaster.Enums;

public enum RenameTargetType
{
    [Description("文件")]
    File,
        
    [Description("文件夹")]
    Folder
}