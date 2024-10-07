using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using ArchiveMaster.Basic;

namespace ArchiveMaster.ViewModels
{
    public partial class TreeDirInfo : TreeFileDirInfo
    {
        private TreeDirInfo(DirectoryInfo dir, string topDir, TreeDirInfo parent, int depth, int index)
            : base(dir, topDir, parent, depth, index)
        {
        }


        [JsonIgnore]
        public IList<TreeFileDirInfo> Subs { get; } = new List<TreeFileDirInfo>();

        public IList<TreeFileInfo> SubFiles { get; } = new List<TreeFileInfo>();
        public IList<TreeDirInfo> SubDirs { get; } = new List<TreeDirInfo>();

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
                var childDir = new TreeFileInfo(dir, parentDir.TopDirectory, parentDir, parentDir.Depth + 1, index++);
                parentDir.SubFiles.Add(childDir);
                parentDir.Subs.Add(childDir);
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
                parentDir.SubDirs.Add(childDir);
                parentDir.Subs.Add(childDir);

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