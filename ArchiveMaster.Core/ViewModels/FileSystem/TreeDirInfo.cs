using System.Text.Json.Serialization;

namespace ArchiveMaster.ViewModels
{
    public partial class TreeDirInfo : TreeFileDirInfo
    {
        private TreeDirInfo()
        {
        }

        private TreeDirInfo(SimpleFileInfo dir, TreeDirInfo parent, int depth, int index)
            : base(dir, parent, depth, index)
        {
        }

        private TreeDirInfo(DirectoryInfo dir, string topDir, TreeDirInfo parent, int depth, int index)
            : base(dir, topDir, parent, depth, index)
        {
        }

        [JsonIgnore]
        public IReadOnlyList<TreeFileDirInfo> Subs => subs.AsReadOnly();

        private List<TreeFileDirInfo> subs = new List<TreeFileDirInfo>();
        public IReadOnlyList<TreeFileInfo> SubFiles => subFiles.AsReadOnly();
        private List<TreeFileInfo> subFiles = new List<TreeFileInfo>();
        public IReadOnlyList<TreeDirInfo> SubDirs => subDirs.AsReadOnly();
        private List<TreeDirInfo> subDirs = new List<TreeDirInfo>();
        private Dictionary<string, TreeDirInfo> subDirsDic = new Dictionary<string, TreeDirInfo>();

        public int SubFileCount { get; set; }
        public int SubFolderCount { get; set; }

        [JsonIgnore]
        public bool IsExpanded { get; set; }

        public static TreeDirInfo BuildTree(string rootDir)
        {
            TreeDirInfo root = new TreeDirInfo(new DirectoryInfo(rootDir), rootDir, null, 0, 0);
            EnumerateDirsAndFiles(root, CancellationToken.None);
            return root;
        }

        public static TreeDirInfo CreateEmptyTree()
        {
            return new TreeDirInfo();
        }

        private char[] pathSeparator = ['/', '\\'];

        public void AddFile(SimpleFileInfo file)
        {
            var relativePath = file.RelativePath;
            var parts = relativePath.Split(pathSeparator, StringSplitOptions.RemoveEmptyEntries);
            Add(parts, file, 0);
        }

        private void Add(string[] pathParts, SimpleFileInfo file, int depth)
        {
            //这里的depth和this.Depth可能不同，比如在非顶级目录调用Add方法
            if (depth == pathParts.Length - 1)
            {
                TreeFileInfo treeFile = new TreeFileInfo(file, this, Depth + 1, subs.Count);
                AddSub(treeFile);
                return;
            }

            string name = pathParts[depth];
            if (!subDirsDic.TryGetValue(name, out TreeDirInfo subDir))
            {
                subDir = new TreeDirInfo()
                {
                    Name = name,
                    TopDirectory = TopDirectory,
                    Path = Path == null ? name : System.IO.Path.Combine(Path, name),
                    Depth = Depth + 1,
                    Index = subs.Count,
                    IsDir = true,
                };
                AddSub(subDir);
            }

            subDir.Add(pathParts, file, depth + 1);
        }

        private void AddSub(TreeFileDirInfo item)
        {
            switch (item)
            {
                case TreeFileInfo file:
                    subFiles.Add(file);
                    subs.Add(file);
                    break;

                case TreeDirInfo dir:
                    subDirs.Add(dir);
                    subs.Add(dir);
                    subDirsDic.Add(dir.Name, dir);
                    break;

                default:
                    throw new ArgumentException("未知的类习惯");
            }
        }

        public static async Task<TreeDirInfo> BuildTreeAsync(string rootDir,
            CancellationToken cancellationToken = default)
        {
            TreeDirInfo root = new TreeDirInfo(new DirectoryInfo(rootDir), rootDir, null, 0, 0);
            await Task.Run(() => EnumerateDirsAndFiles(root, cancellationToken), cancellationToken);
            return root;
        }

        private static int EnumerateFiles(TreeDirInfo parentDir, int initialIndex, CancellationToken cancellationToken)
        {
            int index = initialIndex;
            int count = 0;
            foreach (var dir in (parentDir.FileSystemInfo as DirectoryInfo).EnumerateFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var childFile = new TreeFileInfo(dir, parentDir.TopDirectory, parentDir, parentDir.Depth + 1, index++);
                parentDir.AddSub(childFile);
                count++;
            }

            while (parentDir != null)
            {
                parentDir.SubFileCount += count;
                parentDir = parentDir.Parent;
            }

            return index;
        }

        private static void EnumerateDirsAndFiles(TreeDirInfo dir, CancellationToken cancellationToken)
        {
            int tempIndex = EnumerateDirs(dir, 0, cancellationToken);
            EnumerateFiles(dir, tempIndex, cancellationToken);
        }

        private static int EnumerateDirs(TreeDirInfo parentDir, int initialIndex, CancellationToken cancellationToken)
        {
            int index = initialIndex;
            int count = 0;
            foreach (var dir in (parentDir.FileSystemInfo as DirectoryInfo).EnumerateDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var childDir = new TreeDirInfo(dir, parentDir.TopDirectory, parentDir, parentDir.Depth + 1, index++);
                parentDir.AddSub(childDir);

                try
                {
                    EnumerateDirsAndFiles(childDir, cancellationToken);
                }
                catch (UnauthorizedAccessException ex)
                {
                    childDir.Warn("没有访问权限");
                }
                catch (Exception ex)
                {
                    childDir.Warn("枚举子文件和目录失败：" + ex.Message);
                }

                count++;
            }

            while (parentDir != null)
            {
                parentDir.SubFolderCount += count;
                parentDir = parentDir.Parent;
            }

            return index;
        }

        public IEnumerable<SimpleFileInfo> Flatten(bool includingDir = false)
        {
            Stack<TreeDirInfo> stack = new Stack<TreeDirInfo>();
            stack.Push(this);
            // List<SimpleFileInfo> files = new List<SimpleFileInfo>();
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (includingDir)
                {
                    yield return current;
                    // files.Add(current);
                }

                foreach (var subFile in current.SubFiles)
                {
                    yield return subFile;
                    // files.Add(subFile);
                }

                foreach (var subDir in current.SubDirs.Reverse())
                {
                    stack.Push(subDir);
                }
            }

            // return files;
        }
    }
}