namespace ArchiveMaster.Enums;

public enum SnapshotType
{
    /// <summary>
    /// 全量快照，完整备份每一个文件
    /// </summary>
    Full,
    /// <summary>
    /// 虚拟快照，当设置仅备份差异文件时，首次进行虚拟备份，仅将元数据写入数据库，不进行真正的文件复制
    /// </summary>
    VirtualFull,
    /// <summary>
    /// 增量快照，在已有全量快照的前提下，仅备份和记录与上一个快照有差异的部分
    /// </summary>
    Increment
}