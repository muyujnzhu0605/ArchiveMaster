using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiveMaster.UI
{
    public interface IModuleInitializer
    {
        public void RegisterConfigs();
        public void RegisterViews();
        public string ModuleName { get; }
    }
}
