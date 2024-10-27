using System.ComponentModel;

namespace ArchiveMaster.Enums;

public enum BackupTaskStatus
{
    [Description("就绪")]
    Ready,
    [Description("正在全量备份")]
    FullBackingUp,
    [Description("正在增量备份")]
    IncrementBackingUp,
    [Description("错误")]
    Error
}