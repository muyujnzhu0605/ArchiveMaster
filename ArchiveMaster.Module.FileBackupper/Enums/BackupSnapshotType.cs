namespace ArchiveMaster.Enums;

public enum BackupSnapshotType
{
    /// <summary>
    /// 全量备份
    /// </summary>
    Full,
    
    /// <summary>
    /// 增量备份
    /// </summary>
    Incremental,
    
    /// <summary>
    /// 虚拟快照，当设置仅备份差异文件时，首次进行虚拟备份，仅将元数据写入数据库，不进行真正的文件复制
    /// </summary>
    Virtual
}