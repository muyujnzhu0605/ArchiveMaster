using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using ArchiveMaster.ViewModels.FileSystemViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ArchiveMaster.Utilities
{
    public class TimeClassifyUtility(TimeClassifyConfig config) : TwoStepUtilityBase
    {
        public TimeClassifyConfig Config { get; init; } = config;
        public List<SimpleDirInfo> TargetDirs { get; set; }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(TargetDirs, nameof(TargetDirs));
            await Task.Run(() =>
            {
                int index = 0;
                foreach (var dir in TargetDirs)
                {
                    token.ThrowIfCancellationRequested();
                    index++;
                    NotifyProgressUpdate(TargetDirs.Count, index, $"正在移动（{index}/{TargetDirs.Count}）");
                    string newDirName = dir.EarliestTime.ToString("yyyyMMdd-HHmmss");
                    string newDirPath = Path.Combine(Config.Dir, newDirName);
                    Directory.CreateDirectory(newDirPath);
                    foreach (var sub in dir.Subs)
                    {
                        string targetPath = Path.Combine(newDirPath, sub.Name);
                        Debug.WriteLine($"{sub.Path} => {targetPath}");
                        if (sub is SimpleDirInfo d)
                        {
                            Directory.Move(sub.Path, targetPath);
                        }
                        else if (sub is SimpleFileInfo f)
                        {
                            File.Move(sub.Path, targetPath);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }, token);
        }

        public override async Task InitializeAsync()
        {
            List<SimpleFileInfo> files = null;
            List<SimpleDirInfo> subDirs = null;
            List<SimpleDirInfo> targetDirs = new List<SimpleDirInfo>();

            await Task.Run(() =>
            {
                NotifyProgressUpdate(0, -1, "正在搜索文件");
                files = Directory.EnumerateFiles(Config.Dir)
                    .Select(p => new SimpleFileInfo(p))
                    .OrderBy(p => p.Time)
                    .ToList();
                subDirs = Directory.EnumerateDirectories(Config.Dir)
                    .Select(p => new SimpleDirInfo(p))
                    .Where(p => p.FilesCount > 0)
                    .OrderBy(p => p.EarliestTime)
                    .ToList();
                if (files.Count == 0 && subDirs.Count == 0)
                {
                    throw new Exception("目录为空");
                }

                NotifyProgressUpdate(0, -1, "正在分配目录");
                DateTime time = DateTime.MinValue;
                int filesIndex = 0;
                int dirsIndex = 0;
                while (filesIndex < files.Count || dirsIndex < subDirs.Count)
                {
                    var file = filesIndex < files.Count ? files[filesIndex] : null;
                    var dir = dirsIndex < subDirs.Count ? subDirs[dirsIndex] : null;
                    //没有未处理目录，或文件的时间早于目录中最早文件的时间
                    if (dir == null || (file != null && file.Time <= dir.EarliestTime))
                    {
                        //如果和上一个的时间间隔超过了阈值，那么新建目录存放
                        if (file.Time - time > Config.MinTimeInterval)
                        {
                            SimpleDirInfo newDir = new SimpleDirInfo();
                            targetDirs.Add(newDir);
                        }
                        targetDirs[^1].Subs.Add(file);
                        time = time < file.Time ? file.Time : time;//存在dir.LatestTime>file.Time的可能
                        filesIndex++;
                    }
                    else if (file == null || (dir != null && dir.EarliestTime <= file.Time))
                    {
                        if (dir.EarliestTime - time > Config.MinTimeInterval)
                        {
                            SimpleDirInfo newDir = new SimpleDirInfo();
                            targetDirs.Add(newDir);
                        }
                        targetDirs[^1].Subs.Add(dir);
                        time = dir.LatestTime;
                        dirsIndex++;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            });

            foreach (var dir in targetDirs)
            {
                dir.EarliestTime = dir.Subs.Select(p =>
                {
                    if (p is SimpleDirInfo d)
                    {
                        return d.EarliestTime;
                    }
                    else if (p is SimpleFileInfo f)
                    {
                        return f.Time;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }).Min();
                dir.LatestTime = dir.Subs.Select(p =>
                {
                    if (p is SimpleDirInfo d)
                    {
                        return d.EarliestTime;
                    }
                    else if (p is SimpleFileInfo f)
                    {
                        return f.Time;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }).Max();
                dir.Name = $"{dir.EarliestTime:yyyy-MM-dd HH:mm:ss} ~ {dir.LatestTime:yyyy-MM-dd HH:mm:ss}";
            }
            TargetDirs = targetDirs;
        }
    }
}
