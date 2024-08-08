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
        [ObservableProperty]
        private long length;
        
        [ObservableProperty]
        private DateTime time;

        public SimpleFileInfo() : base()
        {

        }
        public SimpleFileInfo(FileInfo file) : base(file)
        {
            Length = file.Length;
            Time = file.LastWriteTime;
        }
    }
}
