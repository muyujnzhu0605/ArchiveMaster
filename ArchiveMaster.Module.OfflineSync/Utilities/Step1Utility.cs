using ArchiveMaster.Models;
using ArchiveMaster.ViewModels;
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
            
            NotifyProgressIndeterminate();

            await Task.Run(() =>
            {
                foreach (var dir in Config.SyncDirs)
                {
                    TryForFiles(new DirectoryInfo(dir)
                        .EnumerateFiles("*", SearchOption.AllDirectories)
                        .Select(p => new SyncFileInfo(p, dir)), (file, s) =>
                    {
                        syncFiles.Add(file);
                        index++;
                        if (index % 10 == 0)
                        {
                            NotifyMessage($"正在搜索：{dir}，已加入 {index} 个文件");
                        }
                    }, token, new FilesLoopOptions(true, AutoApplyProgressMode.None));
                }


                NotifyMessage($"正在保存快照");

                Step1Model model = new Step1Model()
                {
                    Files = syncFiles.ToList(),
                };
                ZipUtility.WriteToZip(model, Config.OutputFile);
            }, token);
        }
    }
}