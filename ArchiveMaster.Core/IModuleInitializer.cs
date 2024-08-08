using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using Avalonia;

namespace ArchiveMaster
{
    public interface IModuleInitializer
    {
        public IList<ConfigInfo> Configs { get; }
        public ToolPanelGroupInfo Views { get; }
        public void RegisterMessages(Visual visual);
        public string ModuleName { get; }
        public int Order { get; }
    }
}
