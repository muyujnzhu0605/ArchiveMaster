using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchiveMaster.Basic;

namespace ArchiveMaster.ViewModels
{
    public partial class TreeDirInfo : TreeFileDirInfo
    {
        private TreeDirInfo(DirectoryInfo dir, string topDir, TreeDirInfo parent, int depth, int index)
            : base(dir, topDir, parent, depth, index)
        {
        }

        public IList<TreeFileDirInfo> Subs { get; } = new List<TreeFileDirInfo>();

        public IList<TreeFileInfo> SubFiles { get; } = new List<TreeFileInfo>();

        public IList<TreeDirInfo> SubDirs { get; } = new List<TreeDirInfo>();

        public int SubFileCount { get; set; }
        public int SubFolderCount { get; set; }

        public bool IsExpanded { get; set; }

        public static TreeDirInfo BuildTree(string rootDir)
        {
            TreeDirInfo root = new TreeDirInfo(new DirectoryInfo(rootDir), rootDir, null, 0, 0);
            EnumerateDirsAndFiles(root);
            return root;
        }

        private static int EnumerateFiles(TreeDirInfo parentDir, int initialIndex)
        {
            int index = initialIndex;
            int count = 0;
            foreach (var dir in (parentDir.FileSystemInfo as DirectoryInfo).EnumerateFiles())
            {
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

        private static void EnumerateDirsAndFiles(TreeDirInfo dir)
        {
            int tempIndex = EnumerateDirs(dir, 0);
            EnumerateFiles(dir, tempIndex);
        }

        private static int EnumerateDirs(TreeDirInfo parentDir, int initialIndex)
        {
            int index = initialIndex;
            int count = 0;
            foreach (var dir in (parentDir.FileSystemInfo as DirectoryInfo).EnumerateDirectories())
            {
                var childDir = new TreeDirInfo(dir, parentDir.TopDirectory, parentDir, parentDir.Depth + 1, index++);
                parentDir.SubDirs.Add(childDir);
                parentDir.Subs.Add(childDir);

                try
                {
                    EnumerateDirsAndFiles(childDir);
                }
                catch (UnauthorizedAccessException ex)
                {
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
    }
}