using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using ArchiveMaster.Views;
using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster;

public partial class App : Application
{
    public override void Initialize()
    {
        new Initializer().Initialize();

        AvaloniaXamlLoader.Load(this);
        if (OperatingSystem.IsWindows())
        {
            Resources.Add("ContentControlThemeFontFamily", new FontFamily("Microsoft YaHei"));
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.Provider.GetRequiredService<MainWindow>();
            desktop.Exit += Desktop_Exit;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = Services.Provider.GetRequiredService<MainView>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void Desktop_Exit(object sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Exit?.Invoke(sender, e);
        AppConfig.Instance.Save();
    }

    public event EventHandler<ControlledApplicationLifetimeExitEventArgs> Exit;
}