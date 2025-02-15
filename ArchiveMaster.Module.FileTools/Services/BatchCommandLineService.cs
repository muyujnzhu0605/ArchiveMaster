using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Enums;
using ArchiveMaster.Helpers;
using ArchiveMaster.ViewModels.FileSystem;
using FilesTimeDirInfo = ArchiveMaster.ViewModels.FileSystem.FilesTimeDirInfo;

namespace ArchiveMaster.Services
{
    public class BatchCommandLineService(AppConfig appConfig)
        : TwoStepServiceBase<BatchCommandLineConfig>(appConfig)
    {
        public List<BatchCommandLineFileInfo> Files { get; set; }

        public override Task ExecuteAsync(CancellationToken token)
        {
            return Task.CompletedTask;
            // return TryForFilesAsync(TargetDirs, (dir, s) => { NotifyMessage($"正在移动{s.GetFileNumberMessage()}"); },
            //     token, FilesLoopOptions.Builder().AutoApplyStatus().AutoApplyFileNumberProgress().Build());
        }

        private List<FileSystemInfo> SearchSpecialLevelFiles(DirectoryInfo dir, int lastLevelCount)
        {
            if (lastLevelCount > 0)
            {
                return dir.EnumerateDirectories("*", FileEnumerateExtension.GetEnumerationOptions(false))
                    .SelectMany(p => SearchSpecialLevelFiles(p, lastLevelCount - 1))
                    .ToList();
            }

            return (Config.Target switch
            {
                BatchTarget.SpecialLevelDirs => dir.EnumerateFiles("*",
                    FileEnumerateExtension.GetEnumerationOptions(false)),
                BatchTarget.SpecialLevelFiles => dir.EnumerateDirectories("*",
                    FileEnumerateExtension.GetEnumerationOptions(false)),
                BatchTarget.SpecialLevelElements => dir.EnumerateFileSystemInfos("*",
                    FileEnumerateExtension.GetEnumerationOptions(false)),
                _ => throw new ArgumentOutOfRangeException()
            }).ToList();
        }

        private List<BatchCommandLineFileInfo> SearchFiles()
        {
            var dir = new DirectoryInfo(Config.Dir);
            IEnumerable<FileSystemInfo> files = Config.Target switch
            {
                BatchTarget.EachFiles => dir.EnumerateFiles("*", FileEnumerateExtension.GetEnumerationOptions()),
                BatchTarget.EachDirs => dir.EnumerateDirectories("*", FileEnumerateExtension.GetEnumerationOptions()),
                BatchTarget.EachElement => dir.EnumerateFileSystemInfos("*",
                    FileEnumerateExtension.GetEnumerationOptions()),
                BatchTarget.TopFiles => dir.EnumerateFiles("*", FileEnumerateExtension.GetEnumerationOptions(false)),
                BatchTarget.TopDirs => dir.EnumerateDirectories("*",
                    FileEnumerateExtension.GetEnumerationOptions(false)),
                BatchTarget.TopElements => dir.EnumerateFileSystemInfos("*",
                    FileEnumerateExtension.GetEnumerationOptions(false)),
                BatchTarget.SpecialLevelDirs => SearchSpecialLevelFiles(dir, Config.Level),
                BatchTarget.SpecialLevelFiles => SearchSpecialLevelFiles(dir, Config.Level),
                BatchTarget.SpecialLevelElements => SearchSpecialLevelFiles(dir, Config.Level),
                _ => throw new ArgumentOutOfRangeException()
            };
            return files.Select(p => new BatchCommandLineFileInfo(p, Config.Dir, Config.Arguments)).ToList();
        }

        public override async Task InitializeAsync(CancellationToken token)
        {
            List<BatchCommandLineFileInfo> files = null;
            await Task.Run(() =>
            {
                NotifyMessage("正在搜索文件");
                files = SearchFiles();
            }, token);
            Files = files;
        }
    }
}