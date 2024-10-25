using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiveMaster.Configs;
using ArchiveMaster.Platforms;
using ArchiveMaster.Utilities;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using FzLib.Avalonia.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ArchiveMaster;

public static class Initializer
{
    private static List<ToolPanelGroupInfo> views = new List<ToolPanelGroupInfo>();
    private static bool stopped = false;
    public static IHost AppHost { get; private set; }

    public static Task StopAsync()
    {
        if (!stopped)
        {
            stopped = true;
            return AppHost.StopAsync();
        }

        return Task.CompletedTask;
    }

    public static IModuleInitializer[] ModuleInitializers { get; } =
    [
        new FileToolsModuleInitializer(),
        new PhotoArchiveModuleInitializer(),
        new OfflineSyncModuleInitializer(),
        new DiscArchiveModuleInitializer(),
        new FileBackupperModuleInitializer(),
    ];

    public static IReadOnlyList<ToolPanelGroupInfo> Views => views.AsReadOnly();

    public static void Initialize()
    {
        if (AppHost != null)
        {
            throw new InvalidOperationException("已经初始化");
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var builder = Host.CreateApplicationBuilder();
        var config = new AppConfig();
        InitializeModules(builder.Services, config);
        config.Load(builder.Services);
        builder.Services.AddSingleton(config);
        builder.Services.AddTransient<MainWindow>();
        builder.Services.AddTransient<MainView>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddHostedService<AppLifetime>();
        builder.Services.TryAddStartupManager();
        AppHost = builder.Build();
        Services.Initialize(AppHost.Services);
        AppHost.Start();
    }

    private static void InitializeModules(IServiceCollection services, AppConfig appConfig)
    {
        List<(int Order, ToolPanelGroupInfo Group)> viewsWithOrder = new List<(int, ToolPanelGroupInfo)>();

        foreach (var moduleInitializer in ModuleInitializers)
        {
            moduleInitializer.RegisterServices(services);

            try
            {
                if (moduleInitializer.Configs != null)
                {
                    foreach (var config in moduleInitializer.Configs)
                    {
                        appConfig.RegisterConfig(config);
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

    public static void ClearViewsInstance()
    {
        foreach (var group in views)
        {
            foreach (var panel in group.Panels)
            {
                panel.PanelInstance = null;
            }
        }
    }
}