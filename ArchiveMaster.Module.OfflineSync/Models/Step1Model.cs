using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiveMaster.ViewModels;
using SyncFileInfo = ArchiveMaster.ViewModels.FileSystem.SyncFileInfo;

namespace ArchiveMaster.Models
{
    public class Step1Model
    {
        public List<SyncFileInfo> Files { get; set; }
    }
}
