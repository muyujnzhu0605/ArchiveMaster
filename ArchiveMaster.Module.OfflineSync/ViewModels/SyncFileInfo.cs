using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ArchiveMaster.Enums;
using FzLib;

namespace ArchiveMaster.ViewModels;

public class SyncFileInfo : FileInfoWithStatus
{
    public SyncFileInfo()
    {
    }

    public SyncFileInfo(FileInfo file, string topDir) : this()
    {
        Name = file.Name;
        Path = System.IO.Path.GetRelativePath(topDir, file.FullName);
        TopDirectory = topDir;
        Time = file.LastWriteTime;
        Length = file.Length;
    }

    /// <summary>
    /// 生成补丁时文件所使用的临时名称
    /// </summary>
    public string TempName { get; set; }

    /// <summary>
    /// 文件更新类型
    /// </summary>
    public FileUpdateType UpdateType { get; set; }

    /// <summary>
    /// 对于 <see cref="UpdateType"/>为<see cref="FileUpdateType.Move"/> 类型的对象，表示异地的相对路径
    /// </summary>
    public string OldPath { get; set; }

    /// <summary>
    /// 异地中，<see cref="SyncFileInfo.Path"/>的最高级目录的真实绝对路径
    /// </summary>
    public string TopDirectory { get; set; }
}
