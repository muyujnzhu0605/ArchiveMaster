using System.ComponentModel;

namespace ArchiveMaster.Enums
{
    public enum DeleteMode
    {
        [Description("直接删除")]
        Delete,
        [Description("移动到删除文件夹")]
        MoveToDeletedFolder
    }
}
