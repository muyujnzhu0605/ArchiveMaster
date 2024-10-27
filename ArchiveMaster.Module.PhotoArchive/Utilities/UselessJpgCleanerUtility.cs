using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveMaster.Utilities
{
    public class UselessJpgCleanerUtility(UselessJpgCleanerConfig config, AppConfig appConfig)
        : TwoStepUtilityBase<UselessJpgCleanerConfig>(config, appConfig)
    {
        public List<SimpleFileInfo> DeletingJpgFiles { get; set; }

        public override Task ExecuteAsync(CancellationToken token)
        {
            var files = DeletingJpgFiles.Where(p => p.IsChecked).ToList();
            return TryForFilesAsync(files, (file, s) =>
            {
                NotifyMessage($"正在删除JPG{s.GetFileNumberMessage()}：{file.Name}");
                File.Delete(file.Path);
            }, token, FilesLoopOptions.Builder().AutoApplyStatus().AutoApplyFileNumberProgress().Build());
        }

        public override Task InitializeAsync(CancellationToken token)
        {
            DeletingJpgFiles = new List<SimpleFileInfo>();
            var jpgs = new DirectoryInfo(Config.Dir)
                .EnumerateFiles("*.jp*g", SearchOption.AllDirectories)
                .Where(p => p.Name.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
                            p.Name.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase))
                .Select(p => new SimpleFileInfo(p, Config.Dir));
            return TryForFilesAsync(jpgs, (file, s) =>
            {
                NotifyMessage($"正在查找JPG和RAW文件{s.GetFileNumberMessage()}");
                var rawFile =
                    $"{Path.Combine(Path.GetDirectoryName(file.Path), Path.GetFileNameWithoutExtension(file.Name))}.{Config.RawExtension}";
                if (File.Exists(rawFile))
                {
                    DeletingJpgFiles.Add(file);
                }
            }, token, FilesLoopOptions.DoNothing());
        }
    }
}