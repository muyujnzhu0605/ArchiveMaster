using Avalonia;

namespace ArchiveMaster.UI
{
    public interface IModuleInitializer
    {
        public void RegisterConfigs();
        public void RegisterViews();
        public void RegisterMessages(Visual visual);
        public string ModuleName { get; }
    }
}
