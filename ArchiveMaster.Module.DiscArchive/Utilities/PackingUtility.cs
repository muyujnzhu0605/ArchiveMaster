using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.ViewModels;
using DiscUtils.Iso9660;

namespace ArchiveMaster.Utilities
{
    public class PackingUtility(PackingConfig config) : DiscUtilityBase
    {
        public override PackingConfig Config { get; } = config;

        /// <summary>
        /// 根据时间顺序从早到晚排序后的文件
        /// </summary>
        List<FileInfo> filesOrderedByTime = new List<FileInfo>();

        public bool HasMore { get; private set; }

        /// <summary>
        /// 光盘文件包
        /// </summary>
        public DiscFilePackageCollection Packages { get; private set; }

        public override async Task InitializeAsync(CancellationToken token)
        {
            var blacks = new BlackListUtility(Config.BlackList, Config.BlackListUseRegex);
            DiscFilePackageCollection packages = new DiscFilePackageCollection();

            await Task.Run(() =>
            {
                NotifyProgressUpdate("正在搜索文件");
                filesOrderedByTime = new DirectoryInfo(Config.SourceDir)
                    .EnumerateFiles("*", SearchOption.AllDirectories)
                    .Where(p => p.LastWriteTime > Config.EarliestTime)
                    .Where(p=>!blacks.IsInBlackList(p))
                    .OrderBy(p => p.LastWriteTime).ToList();

                packages.DiscFilePackages.Add(new DiscFilePackage());
                long maxSize = 1L * 1024 * 1024 * Config.DiscSizeMB;
                foreach (var file in filesOrderedByTime)
                {
                    token.ThrowIfCancellationRequested();
                    DiscFile discFile = new DiscFile(file);

                    //文件超过单盘大小
                    if (file.Length > maxSize)
                    {
                        packages.SizeOutOfRangeFiles.Add(discFile);
                        continue;
                    }

                    //文件超过剩余空间
                    var package = packages.DiscFilePackages[^1];
                    if (file.Length > maxSize - package.TotalLength)
                    {
                        package.EarliestTime = package.Files[0].Time;
                        package.LatestTime = package.Files[^1].Time;
                        package.Index = packages.DiscFilePackages.Count;
                        if (packages.DiscFilePackages.Count >= Config.MaxDiscCount)
                        {
                            Packages = packages;
                            HasMore = true;
                        }

                        package = new DiscFilePackage();
                        packages.DiscFilePackages.Add(package);
                    }

                    //加入文件
                    package.Files.Add(discFile);
                    package.TotalLength += file.Length;
                }

                //处理最后一个
                var lastPackage = packages.DiscFilePackages[^1];
                lastPackage.EarliestTime = lastPackage.Files[0].Time;
                lastPackage.LatestTime = lastPackage.Files[^1].Time;
                lastPackage.Index = packages.DiscFilePackages.Count;
            }, token);
            Packages = packages;
            HasMore = false;
        }


        public override async Task ExecuteAsync(CancellationToken token)
        {
            if (!Directory.Exists(Config.TargetDir))
            {
                Directory.CreateDirectory(Config.TargetDir);
            }

            long length = 0;
            await Task.Run(() =>
            {
                long totalLength = Packages.DiscFilePackages
                    .Where(p => p.IsChecked && p.Index > 0)
                    .Sum(p => p.Files.Sum(q => q.Length));
                foreach (var package in Packages.DiscFilePackages.Where(p => p.IsChecked && p.Index > 0))
                {
                    string dir = Path.Combine(Config.TargetDir, package.Index.ToString());
                    Directory.CreateDirectory(dir);
                    string fileListName = $"filelist-{DateTime.Now:yyyyMMddHHmmss}.txt";
                    CDBuilder builder = null;
                    if (Config.PackingType == PackingType.ISO)
                    {
                        builder = new CDBuilder();
                        builder.UseJoliet = true;
                    }

                    using (var fileListStream = File.OpenWrite(Path.Combine(dir, fileListName)))
                    using (var writer = new StreamWriter(fileListStream))
                    {
                        writer.WriteLine(
                            $"{package.EarliestTime.ToString(DateTimeFormat)}\t{package.LatestTime.ToString(DateTimeFormat)}\t{package.TotalLength}");


                        foreach (var file in package.Files)
                        {
                            length += file.Length;

                            try
                            {
                                var relativePath = Path.GetRelativePath(Config.SourceDir, file.Path);
                                string newName = relativePath.Replace(":", "#c#").Replace("\\", "#s#");
                                string md5 = null;
                                NotifyProgressUpdate(totalLength, length,
                                    $"正在复制第 {package.Index} 个光盘文件包中的 {relativePath}");

                                switch (Config.PackingType)
                                {
                                    case PackingType.Copy:
                                        NotifyProgressUpdate(totalLength, length,
                                            $"正在复制第 {package.Index} 个光盘文件包中的 {relativePath}");
                                        md5 = CopyAndGetHash(file.Path, Path.Combine(dir, newName));
                                        break;
                                    case PackingType.ISO:
                                        builder!.AddFile(newName, file.Path);
                                        md5 = GetMD5(file.Path);
                                        break;
                                    case PackingType.HardLink:
                                        CreateHardLink(Path.Combine(dir, newName), file.Path);
                                        md5 = GetMD5(file.Path);
                                        break;
                                }

                                writer.WriteLine(
                                    $"{newName}\t{relativePath}\t{file.Time.ToString(DateTimeFormat)}\t{file.Length}\t{md5}");
                                file.Complete = true;
                            }
                            catch (Exception ex)
                            {
                                file.Message = ex.Message;
                            }
                        }
                    }

                    if (Config.PackingType == PackingType.ISO)
                    {
                        NotifyProgressUpdate($"正在创第 {package.Index} 个ISO");
                        builder.AddFile(fileListName, Path.Combine(dir, fileListName));
                        builder.Build(Path.Combine(Path.GetDirectoryName(dir), Path.GetFileName(dir) + ".iso"));
                    }
                }
            }, token);
        }


        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName,
            IntPtr lpSecurityAttributes);

        private void CreateHardLink(string link, string source)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException(source);
            }

            if (File.Exists(link))
            {
                File.Delete(link);
            }

            if (Path.GetPathRoot(link) != Path.GetPathRoot(source))
            {
                throw new IOException("硬链接的两者必须在同一个分区中");
            }

            bool value = CreateHardLink(link, source, IntPtr.Zero);
            if (!value)
            {
                throw new IOException("未知错误，无法创建硬链接");
            }
        }
    }
}