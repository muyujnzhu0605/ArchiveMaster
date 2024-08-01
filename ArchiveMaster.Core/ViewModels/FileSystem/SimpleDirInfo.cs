using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArchiveMaster.ViewModels
{
    public partial class SimpleDirInfo : SimpleFileOrDirInfo
    {
        public SimpleDirInfo()
        {

        }
        public SimpleDirInfo(string path) : base(path)
        {
            var subFiles = Directory.EnumerateFiles(path).ToList();
            Subs = subFiles.Select(p => new SimpleFileOrDirInfo(p)).ToList();
            FilesCount = subFiles.Count;
            if (FilesCount > 0)
            {
                EarliestTime = new DateTime(subFiles
                    .Select(File.GetLastWriteTime)
                    .Select(p => p.Ticks)
                    .Min());
                LatestTime = new DateTime(subFiles
                    .Select(File.GetLastWriteTime)
                    .Select(p => p.Ticks)
                    .Max());
            }
        }

        [ObservableProperty]
        private int filesCount;

        [ObservableProperty]
        private DateTime earliestTime;

        [ObservableProperty]
        private DateTime latestTime;

        [ObservableProperty]
        private List<SimpleFileOrDirInfo> subs  = new List<SimpleFileOrDirInfo>();
    }
}
