using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Xml.Serialization;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Helpers;
using ArchiveMaster.ViewModels;
using ArchiveMaster.ViewModels.FileSystem;
using Avalonia.Controls.Platform;
using FzLib.Cryptography;
using FzLib.IO;
using Microsoft.Win32.SafeHandles;

namespace ArchiveMaster.Services
{
    public class DuplicateFileCleanupService(DuplicateFileCleanupConfig config, AppConfig appConfig)
        : TwoStepServiceBase<DuplicateFileCleanupConfig>(config, appConfig)
    {
        public override async Task ExecuteAsync(CancellationToken token)
        {
            // await Task.Run(() =>
            // {
            //     if (!string.IsNullOrWhiteSpace(Config.TargetDir))
            //     {
            //         //TryForFiles(flatten, (file, s) =>
            //         //{
            //         //    NotifyMessage($"正在创建{s.GetFileNumberMessage()}：{file.RelativePath}");
            //         //}, token, FilesLoopOptions.Builder().AutoApplyFileNumberProgress().AutoApplyStatus().Build());
            //     }
            // }, token);
        }

        private FileMatchHelper matcher;

        public override Task InitializeAsync(CancellationToken token)
        {
            List<DuplicateFileInfo> files = new List<DuplicateFileInfo>();

            return Task.Run(() =>
            {
                NotifyMessage("正在枚举文件");
                List<FileInfo> allReferenceFiles = new List<FileInfo>();
                if (Config.CleanUpSelf)
                {
                    allReferenceFiles = new DirectoryInfo(Config.CleaningDir)
                        .EnumerateFiles("*", OptionsHelper.GetEnumerationOptions())
                        .WithCancellationToken(token)
                        .ToList();
                }

                if (Config.CleanUpByReference)
                {
                    allReferenceFiles = allReferenceFiles.Union(new DirectoryInfo(Config.CleaningDir)
                        .EnumerateFiles("*", OptionsHelper.GetEnumerationOptions())
                        .WithCancellationToken(token)).ToList();
                }

                NotifyMessage("正在构建文件特征");
                matcher = BuildFileFeatures(token);
                Match(allReferenceFiles, token);
            }, token);
        }

        private FileMatchHelper BuildFileFeatures(CancellationToken token)
        {
            matcher = new FileMatchHelper(Config.CompareName, Config.CompareLength, Config.CompareTime,
                Config.TimeToleranceSecond);
            matcher.AddReferenceDir(Config.CleaningDir, token);

            return matcher;
        }

        public IReadOnlyList<DuplicateFileInfo> DuplicateFiles { get; private set; }

        private void Match(IEnumerable<FileInfo> referenceFiles, CancellationToken token)
        {
            HashSet<string> checkedFiles = new HashSet<string>();

            NotifyMessage("正在匹配文件");
            List<DuplicateFileInfo> duplicateFiles = new List<DuplicateFileInfo>();
            foreach (var file in referenceFiles)
            {
                token.ThrowIfCancellationRequested();
                if (!checkedFiles.Add(file.FullName))
                {
                    continue;
                }

                var matchedFiles = matcher.GetMatchedFiles(file);
                if (matchedFiles.Count == 0 || matchedFiles.Count == 1 && matchedFiles.Contains(file.FullName))
                {
                    //没有匹配到，或者只匹配到了自己
                    continue;
                }

                foreach (var matchedFile in matchedFiles)
                {
                    if (matchedFile == file.FullName)
                    {
                        continue;
                    }

                    checkedFiles.Add(matchedFile);
                    duplicateFiles.Add(new DuplicateFileInfo(new FileInfo(matchedFile), Config.CleaningDir,
                        file.FullName));
                }
            }

            DuplicateFiles = duplicateFiles.AsReadOnly();
        }
    }
}