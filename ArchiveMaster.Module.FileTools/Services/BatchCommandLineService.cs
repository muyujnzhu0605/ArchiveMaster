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
            return TryForFilesAsync(Files, (file, s) =>
                {
                    NotifyMessage($"正在处理{s.GetFileNumberMessage()}");
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = Config.Program,
                        Arguments = file.CommandLine,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process process = null;
                    token.Register(() =>
                    {
                        try
                        {
                            if (!process.HasExited) // 检查进程是否已经退出
                            {
                                process.Kill(); // 终止进程
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            // 忽略进程未启动或已释放的异常
                        }
                    });
                    using (process = new Process { StartInfo = startInfo })
                    {
                        token.ThrowIfCancellationRequested();
                        process.Start();
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        file.ProcessOutput = output;
                        file.ProcessError = error;
                        if (process.ExitCode != 0)
                        {
                            file.Error($"进程退出代码不为0（为{process.ExitCode}）");
                        }
                    }
                },
                token, FilesLoopOptions.Builder().AutoApplyStatus().AutoApplyFileNumberProgress().Build());
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

        private string ReplaceFilePlaceholder(FilePlaceholderReplacer replacer, SimpleFileInfo file)
        {
            return replacer.GetTargetName(file, fileName =>
            {
                string escapedFileName;
                if (OperatingSystem.IsWindows())
                {
                    // Windows 平台：双引号转义为两个双引号，并用双引号包裹
                    escapedFileName = fileName.Replace("\"", "\"\"");
                }
                else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    // Unix 平台：双引号转义为反斜杠加双引号，并用双引号包裹
                    escapedFileName = fileName.Replace("\"", "\\\"");
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }

                return escapedFileName;
            });
        }

        public override async Task InitializeAsync(CancellationToken token)
        {
            FilePlaceholderReplacer replacer = new FilePlaceholderReplacer(Config.Arguments);
            if (!replacer.HasPattern)
            {
                throw new Exception("命令行参数不包含需要替换的占位符");
            }

            List<BatchCommandLineFileInfo> files = new List<BatchCommandLineFileInfo>();
            await Task.Run(() =>
            {
                NotifyMessage("正在搜索文件");
                var dir = new DirectoryInfo(Config.Dir);
                IEnumerable<FileSystemInfo> fileSystems = Config.Target switch
                {
                    BatchTarget.EachFiles => dir.EnumerateFiles("*", FileEnumerateExtension.GetEnumerationOptions()),
                    BatchTarget.EachDirs => dir.EnumerateDirectories("*",
                        FileEnumerateExtension.GetEnumerationOptions()),
                    BatchTarget.EachElement => dir.EnumerateFileSystemInfos("*",
                        FileEnumerateExtension.GetEnumerationOptions()),
                    BatchTarget.TopFiles => dir.EnumerateFiles("*",
                        FileEnumerateExtension.GetEnumerationOptions(false)),
                    BatchTarget.TopDirs => dir.EnumerateDirectories("*",
                        FileEnumerateExtension.GetEnumerationOptions(false)),
                    BatchTarget.TopElements => dir.EnumerateFileSystemInfos("*",
                        FileEnumerateExtension.GetEnumerationOptions(false)),
                    BatchTarget.SpecialLevelDirs => SearchSpecialLevelFiles(dir, Config.Level),
                    BatchTarget.SpecialLevelFiles => SearchSpecialLevelFiles(dir, Config.Level),
                    BatchTarget.SpecialLevelElements => SearchSpecialLevelFiles(dir, Config.Level),
                    _ => throw new ArgumentOutOfRangeException()
                };

                foreach (var file in fileSystems)
                {
                    var batchFile = new BatchCommandLineFileInfo(file, Config.Dir);
                    batchFile.CommandLine = ReplaceFilePlaceholder(replacer, batchFile);
                    files.Add(batchFile);
                }
            }, token);
            Files = files;
        }
    }
}