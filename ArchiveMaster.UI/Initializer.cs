using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArchiveMaster.Configs;
using ArchiveMaster.Platforms;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ArchiveMaster;

public class Initializer
{
    public void Initialize()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        InitializeModules();
        AppConfig.Instance.Load();
        
        Services.Builder.AddSingleton(this);
        Services.Builder.AddTransient<MainWindow>();
        Services.Builder.AddTransient<MainView>();
        Services.Builder.AddTransient<MainViewModel>();
        
        Services.BuildServiceProvider();
    }

    public IModuleInitializer[] ModuleInitializers { get; } =
    [
        new FileToolsModuleInitializer(),
        // new PhotoArchiveModuleInitializer(),
        new OfflineSyncModuleInitializer(),
        // new DiscArchiveModuleInitializer(),
    ];

    private void InitializeModules()
    {
        List<(int Order, ToolPanelGroupInfo Group)> viewsWithOrder = new List<(int, ToolPanelGroupInfo)>();
        foreach (var moduleInitializer in ModuleInitializers)
        {
            if (moduleInitializer == null)
            {
                throw new Exception($"模块不存在实现了{nameof(IModuleInitializer)}的类");
            }

            moduleInitializer.RegisterServices(Services.Builder);
      
            try
            {
                if (moduleInitializer.Configs != null)
                {
                    foreach (var config in moduleInitializer.Configs)
                    {
                        AppConfig.RegisterConfig(config.Type, config.Key);
                    }
                }

                if (moduleInitializer.Views != null)
                {
                    viewsWithOrder.Add((moduleInitializer.Order, moduleInitializer.Views));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"加载模块{moduleInitializer.ModuleName}时出错: {ex.Message}");
            }
        }

        views = viewsWithOrder.OrderBy(p => p.Order).Select(p => p.Group).ToList();
        
    }  
    
    private List<ToolPanelGroupInfo> views = new List<ToolPanelGroupInfo>();
    public IReadOnlyList<ToolPanelGroupInfo> Views => views.AsReadOnly();
}