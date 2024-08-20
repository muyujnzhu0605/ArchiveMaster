using FzLib;
using MetadataExtractor;
using ArchiveMaster.Configs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Utilities
{
    public class RepairModifiedTimeUtility(RepairModifiedTimeConfig config)
        : TwoStepUtilityBase<RepairModifiedTimeConfig>(config)
    {
        public string[] Extensions = { "jpg", "jpeg", "heif", "heic" };

        public ConcurrentBag<ExifTimeFileInfo> Files { get; } = new ConcurrentBag<ExifTimeFileInfo>();

        private Regex rRepairTime;

        public override Task ExecuteAsync(CancellationToken token)
        {
            return TryForFilesAsync(Files, (file, s) =>
            {
                if (!file.ExifTime.HasValue)
                {
                    return;
                }

                NotifyMessage($"正在处理{s.GetFileNumberMessage()}：{file.Name}");
                File.SetLastAccessTime(file.Path, file.ExifTime.Value);
            }, token, FilesLoopOptions.Builder().AutoApplyStatus().AutoApplyFileNumberProgress().Build());
        }

        public override Task InitializeAsync(CancellationToken token)
        {
            rRepairTime = new Regex(@$"\.({string.Join('|', Extensions)})$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            NotifyProgressIndeterminate();
            NotifyMessage("正在查找文件");
            var files = new DirectoryInfo(Config.Dir).EnumerateFiles("*", SearchOption.AllDirectories)
                .Select(p => new ExifTimeFileInfo(p));
            return TryForFilesAsync(files, (file, s) =>
                {
                    NotifyMessage($"正在扫描照片日期{s.GetFileNumberMessage()}");
                    if (rRepairTime.IsMatch(file.Name))
                    {
                        DateTime? exifTime = FindExifTime(file.Path);

                        if (exifTime.HasValue)
                        {
                            var fileTime = file.Time;
                            var duration = (exifTime.Value - fileTime).Duration();
                            if (duration > Config.MaxDurationTolerance)
                            {
                                file.ExifTime = exifTime.Value;
                                Files.Add(file);
                            }
                        }
                    }
                }, token,
                FilesLoopOptions.Builder().WithMultiThreads(Config.ThreadCount).Catch((file, ex) =>
                {
                    Files.Add(file as ExifTimeFileInfo);
                }).Build());
        }

        private DateTime? FindExifTime(string file)
        {
            IReadOnlyList<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);
            MetadataExtractor.Directory dir = null;
            if ((dir = directories.FirstOrDefault(p => p.Name == "Exif SubIFD")) != null)
            {
                if (dir.TryGetDateTime(36867, out DateTime time1))
                {
                    return time1;
                }

                if (dir.TryGetDateTime(36868, out DateTime time2))
                {
                    return time2;
                }
            }

            if ((dir = directories.FirstOrDefault(p => p.Name == "Exif IFD0")) != null)
            {
                if (dir.TryGetDateTime(306, out DateTime time))
                {
                    return time;
                }
            }

            return null;
        }
    }
}