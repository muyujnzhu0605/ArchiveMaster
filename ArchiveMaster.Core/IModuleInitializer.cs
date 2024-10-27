using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArchiveMaster
{
    public interface IModuleInitializer
    {
        public IList<ConfigInfo> Configs { get; }
        public ToolPanelGroupInfo Views { get; }
        public void RegisterMessages(Visual visual);
        public string ModuleName { get; }
        public int Order { get; }
        public void RegisterServices(IServiceCollection services);
    }
}
