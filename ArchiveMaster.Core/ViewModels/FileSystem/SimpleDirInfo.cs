using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArchiveMaster.ViewModels
{
    public partial class SimpleDirInfo : SimpleFileInfo
    {
        public SimpleDirInfo()
        {
            IsDir = true;
        }
        public SimpleDirInfo(DirectoryInfo dir) : base(dir)
        {
            var subFiles = dir.EnumerateFiles().ToList();
            Subs = subFiles.Select(p => new SimpleFileInfo(p)).ToList();
            FilesCount = subFiles.Count;
            if (FilesCount > 0)
            {
                EarliestTime = new DateTime(subFiles
                    .Select(p=>p.LastWriteTime)
                    .Select(p => p.Ticks)
                    .Min());
                LatestTime = new DateTime(subFiles
                    .Select(p=>p.LastWriteTime)
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
        private List<SimpleFileInfo> subs  = new List<SimpleFileInfo>();
    }
}
