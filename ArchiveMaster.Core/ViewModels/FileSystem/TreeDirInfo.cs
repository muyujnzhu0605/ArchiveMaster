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
            foreach (var dir in (parentDir.FileSystemInfo as DirectoryInfo).EnumerateFiles())
            {
                var childDir = new TreeFileInfo(dir, parentDir.TopDirectory, parentDir, parentDir.Depth + 1, index++);
                parentDir.SubFiles.Add(childDir);
                parentDir.Subs.Add(childDir);
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
            }

            return index;
        }
    }
}