using ArchiveMaster.Models;
using ArchiveMaster.ViewModels;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;

namespace ArchiveMaster.Utilities
{
    public class Step1Utility(Step1Config config) : TwoStepUtilityBase
    {
        public override Step1Config Config { get; } = config;

        public override Task InitializeAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public override async Task ExecuteAsync(CancellationToken token = default)
        {
            int index = 0;
            List<SyncFileInfo> syncFiles = new List<SyncFileInfo>();
            var groups = Config.SyncDirs.GroupBy(p => Path.GetFileName(p));
            foreach (var group in groups)
            {
                if (group.Count() > 1)
                {
                    throw new ArgumentException("存在重复的顶级目录名：" + group.Key);
                }
            }

            await Task.Run(() =>
            {
                foreach (var dir in Config.SyncDirs)
                {
                    foreach (var file in new DirectoryInfo(dir).EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        token.ThrowIfCancellationRequested();
                        syncFiles.Add(new SyncFileInfo(file, dir));
#if DEBUG
                        TestUtility.SleepInDebug();
#endif
                        NotifyProgressUpdate($"正在搜索：{dir}，已加入 {++index} 个文件");
                    }
                }
            }, token);
            NotifyProgressUpdate($"正在保存快照");

            Step1Model model = new Step1Model()
            {
                Files = syncFiles.ToList(),
            };
            await Task.Run(() =>
            {
                ZipUtility.WriteToZip(model, Config.OutputFile);
                
            }, token);
        }
    }
}