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
        private FileMatchHelper matcher;

        public TreeDirInfo DuplicateGroups { get; private set; }

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

        public override Task InitializeAsync(CancellationToken token)
        {
            List<DuplicateFileInfo> files = new List<DuplicateFileInfo>();
            Config.Check();
            return Task.Run(() =>
            {
                NotifyMessage("正在构建待清理文件特征");
                matcher = BuildFileFeatures(token);

                NotifyMessage("正在枚举参考文件");
                List<SimpleFileInfo> allReferenceFiles =
                    new DirectoryInfo(Config.CleaningDir).GetSimpleFileInfos(Config.ReferenceDir, token);

                NotifyMessage("正在匹配文件");
                MatchAndGroup(allReferenceFiles, token);
            }, token);
        }

        private FileMatchHelper BuildFileFeatures(CancellationToken token)
        {
            matcher = new FileMatchHelper(Config.CompareName, Config.CompareLength, Config.CompareTime,
                Config.TimeToleranceSecond);
            matcher.AddReferenceDir(Config.CleaningDir, token);

            return matcher;
        }

        private void MatchAndGroup(IEnumerable<SimpleFileInfo> referenceFiles, CancellationToken token)
        {
            HashSet<string> checkedFiles = new HashSet<string>();

            List<DuplicateFileInfo> duplicateFiles = new List<DuplicateFileInfo>();
            foreach (var file in referenceFiles)
            {
                token.ThrowIfCancellationRequested();
                if (!checkedFiles.Add(file.Path))
                {
                    continue;
                }

                var matchedFiles = matcher.GetMatchedFiles(file.FileSystemInfo as FileInfo);
                if (matchedFiles.Count == 0 || matchedFiles.Count == 1 && matchedFiles.Contains(file.Path))
                {
                    //没有匹配到，或者只匹配到了自己
                    continue;
                }

                foreach (var matchedFile in matchedFiles)
                {
                    checkedFiles.Add(matchedFile);
                    duplicateFiles.Add(new DuplicateFileInfo(new FileInfo(matchedFile), Config.CleaningDir, file));
                }
            }

            var tree = TreeDirInfo.CreateEmptyTree();
            foreach (var group in duplicateFiles.GroupBy(p => p.ExistedFile, SimpleFileInfo.EqualityComparer))
            {
                token.ThrowIfCancellationRequested();
                var refFile = new TreeDirInfo()
                {
                    Name = group.Key.Name,
                    Path = group.Key.Path,
                    TopDirectory = Config.ReferenceDir,
                    Depth = 1,
                    Index = tree.Subs.Count,
                    IsDir = true,
                };
                tree.AddSub(refFile);
                bool first = true;
                foreach (var file in group.OrderBy(p => p.Path))
                {
                    var treeFile = refFile.AddSubFile(file);
                    treeFile.IsChecked = !(first && Config.CleaningDir == Config.ReferenceDir);
                    first = false;
                }
            }

            DuplicateGroups = tree;
        }
    }
}