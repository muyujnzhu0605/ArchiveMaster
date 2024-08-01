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
    public class UselessJpgCleanerUtility(UselessJpgCleanerConfig config) : TwoStepUtilityBase
    {
        private UselessJpgCleanerConfig Config { get;  } = config;

        public List<SimpleFileInfo> DeletingJpgFiles { get; set; }
        public override Task ExecuteAsync(CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(DeletingJpgFiles);
            return Task.Run(() =>
            {
                int index = 0;
                foreach (var file in DeletingJpgFiles)
                {
                    index++;
                    token.ThrowIfCancellationRequested();
                    NotifyProgressUpdate(DeletingJpgFiles.Count, index, $"正在删除JPG（{index}/{DeletingJpgFiles.Count}）");
                    File.Delete(file.Path);
                }
                DeletingJpgFiles = null;
            }, token);
        }

        public override Task InitializeAsync(CancellationToken token)
        {
            DeletingJpgFiles = new List<SimpleFileInfo>();
            return Task.Run(() =>
            {
                NotifyProgressUpdate(0, -1, "正在搜索JPG文件");
                var jpgs = Directory
                    .EnumerateFiles(Config.Dir, "*.jp*g", SearchOption.AllDirectories)
                    .Where(p => p.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                int index = 0;
                foreach (var jpg in jpgs)
                {
                    token.ThrowIfCancellationRequested();
                    index++;
                    NotifyProgressUpdate(jpgs.Count, index, $"正在查找RAW文件（{index}/{jpgs.Count}）");
                    var rawFile = $"{Path.Combine(Path.GetDirectoryName(jpg), Path.GetFileNameWithoutExtension(jpg))}.{Config.RawExtension}";
                    if (File.Exists(rawFile))
                    {
                        DeletingJpgFiles.Add(new SimpleFileInfo(jpg));
                    }
                }
            }, token);

        }
    }
}
