using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Utilities
{
    public class RebuildUtility(RebuildConfig config) : DiscUtilityBase
    {
        private Dictionary<string, List<DiscFile>> files;

        public override RebuildConfig Config { get; } = config;
        public FileSystemTree FileTree { get; private set; }
        public List<RebuildError> rebuildErrors;
        public IReadOnlyList<RebuildError> RebuildErrors => rebuildErrors.AsReadOnly();

        public override async Task InitializeAsync(CancellationToken token)
        {
            files = ReadFileList(Config.DiscDirs);
            FileSystemTree tree = FileSystemTree.CreateRoot();
            await Task.Run(() =>
            {
                foreach (var dir in files.Keys)
                {
                    foreach (var file in files[dir])
                    {
                        string filePath = Path.Combine(dir, file.DiscName);
                        if (!File.Exists(filePath))
                        {
                            throw new FileNotFoundException(filePath);
                        }

                        var pathParts = file.Path.Split('\\', '/');
                        var current = tree;
                        for (int i = 0; i < pathParts.Length - 1; i++)
                        {
                            var part = pathParts[i];
                            if (current.Directories.Any(p => p.Name == part))
                            {
                                current = current.Directories.First(p => p.Name == part);
                            }
                            else
                            {
                                current = current.AddChild(part);
                            }
                        }

                        var treeFile = current.AddFile(file.Name);
                        treeFile.File = file;
                    }
                }
            }, token);
            FileTree = tree;
        }

        public override Task ExecuteAsync(CancellationToken token)
        {
            rebuildErrors = new List<RebuildError>();
            long length = 0;
            long totalLength = files.Values.Sum(p => p.Sum(q => q.Length));
            int count = 0;
            return Task.Run(() =>
            {
                foreach (var dir in files.Keys)
                {
                    foreach (var file in files[dir])
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            length += file.Length;
                            var srcPath = Path.Combine(dir, file.DiscName);
                            var distPath = Path.Combine(Config.TargetDir, file.Path);
                            var distFileDir = Path.GetDirectoryName(distPath);
                            var name = Path.GetFileName(file.Path);
                            NotifyProgressUpdate(totalLength, length, $"正在重建 {file.Path}");
                            if (!Directory.Exists(distFileDir))
                            {
                                Directory.CreateDirectory(distFileDir);
                            }

                            if (File.Exists(distPath) && Config.SkipIfExisted)
                            {
                                throw new Exception("文件已存在");
                            }

                            string md5;
                            if (Config.CheckOnly)
                            {
                                md5 = GetMD5(srcPath);
                            }
                            else
                            {      
                                md5 = CopyAndGetHash(srcPath, distPath);
                            }

                            if (md5 != file.Md5)
                            {
                                throw new Exception("MD5验证失败");
                            }

                            if ((File.GetLastWriteTime(srcPath) - file.Time).Duration().TotalSeconds >
                                Config.MaxTimeToleranceSecond)
                            {
                                throw new Exception("修改时间不一致");
                            }

                            count++;
                        }
                        catch (Exception ex)
                        {
                            rebuildErrors.Add(new RebuildError(file, ex.Message));
                        }
                    }
                }
            }, token);
        }
    }
}