using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchiveMaster.Basic;
using Mapster;

namespace ArchiveMaster.ViewModels
{
    public partial class TreeDirInfo : SimpleFileInfo
    {
        public TreeDirInfo()
        {
            IsDir = true;
        }

        public TreeDirInfo(SimpleFileInfo file)
        {
            file.Adapt(this);
            IsDir = true;
        }
        
        public TreeDirInfo(DirectoryInfo dir):base(dir)
        {
            IsDir = true;
        }

        public IList<SimpleFileInfo> Subs { get; } = new List<SimpleFileInfo>();

        private IDictionary<string,SimpleFileInfo> subDirNameDic = new Dictionary<string,SimpleFileInfo>();

        private static readonly char[] pathSeparators = new[]
        {
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar
        }.Distinct().ToArray();

        public void AddChildDir(string name)
        {
            
        }

        public void AddToTree(SimpleFileInfo file)
        {
            var relativePath = file.RelativePath;
            var fileParts = relativePath.Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries);
            var dir = this;
            foreach (var part in fileParts[..^2])
            {
                if (dir.subDirNameDic.TryGetValue(part,out SimpleFileInfo subFile) && subFile is TreeDirInfo subDir)
                {
                    dir = subDir;
                }
                else
                {
                    subDir = new TreeDirInfo(file);
                    dir.Subs.Add(subDir);
                    dir.subDirNameDic.Add(subDir.Name,subDir);
                }
            }
            
        }
    }
}