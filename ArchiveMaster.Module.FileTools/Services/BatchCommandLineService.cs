using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        public event DataReceivedEventHandler ProcessDataReceived;

        public override Task ExecuteAsync(CancellationToken token)
        {
            return TryForFilesAsync(Files, async (file, s) =>
                {
                    NotifyMessage($"正在处理{s.GetFileNumberMessage()}");
                    if (file.AutoCreateDir != null)
                    {
                        Directory.CreateDirectory(file.AutoCreateDir);
                    }

                    await RunProcessAsync(token, file, s);
                },
                token, FilesLoopOptions.Builder().AutoApplyStatus().AutoApplyFileNumberProgress().Build());
        }

        private async Task RunProcessAsync(CancellationToken token, BatchCommandLineFileInfo file, FilesLoopStates s)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Config.Program,
                Arguments = file.CommandLine,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage),
                StandardErrorEncoding =  Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage),
            };


            StringBuilder strOutput = new StringBuilder();
            StringBuilder strError = new StringBuilder();

            Process process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    strOutput.AppendLine(e.Data);
                    ProcessDataReceived?.Invoke(this, e);
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    strError.AppendLine(e.Data);
                    ProcessDataReceived?.Invoke(this, e);
                }
            };
            NotifyMessage($"正在运行进程{s.GetFileNumberMessage()}：{file.Name}");


            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(token);

            if (process.ExitCode != 0)
            {
                file.Error($"进程退出代码不为0（为{process.ExitCode}）");
            }

            file.ProcessOutput = strOutput.ToString();
            file.ProcessError = strError.ToString();
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
            FilePlaceholderReplacer argumentsReplacer = new FilePlaceholderReplacer(Config.Arguments);
            if (!argumentsReplacer.HasPattern)
            {
                throw new Exception("命令行参数不包含需要替换的占位符");
            }

            FilePlaceholderReplacer autoCreateDirReplacer = null;
            if (!string.IsNullOrWhiteSpace(Config.AutoCreateDir))
            {
                autoCreateDirReplacer = new FilePlaceholderReplacer(Config.AutoCreateDir);
                if (!argumentsReplacer.HasPattern)
                {
                    throw new Exception("自动创建目录不包含需要替换的占位符");
                }
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
#if DEBUG
                fileSystems = fileSystems.Take(10);
#endif
                foreach (var file in fileSystems)
                {
                    var batchFile = new BatchCommandLineFileInfo(file, Config.Dir);
                    batchFile.CommandLine = ReplaceFilePlaceholder(argumentsReplacer, batchFile);
                    if (autoCreateDirReplacer != null)
                    {
                        batchFile.AutoCreateDir = autoCreateDirReplacer.GetTargetName(batchFile);
                    }

                    files.Add(batchFile);
                }
            }, token);
            Files = files;
        }
    }
}