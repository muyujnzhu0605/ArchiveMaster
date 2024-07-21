using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMaster.ViewModels
{
    public partial class SimpleFileInfo : SimpleFileOrDirInfo
    {
        public SimpleFileInfo(string path) : base(path)
        {
            Time = System.IO.File.GetLastWriteTime(path);
        }

        [ObservableProperty]
        private DateTime time;
    }
}
