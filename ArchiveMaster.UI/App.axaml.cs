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
using Avalonia.Controls;
using FzLib;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster;

public partial class App : Application
{
    private bool isMainWindowOpened = false;

    public event EventHandler<ControlledApplicationLifetimeExitEventArgs> Exit;

    public override void Initialize()
    {
        Initializer.Initialize();

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
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.Exit += Desktop_Exit;

            if (!(desktop.Args is { Length: > 0 } && desktop.Args[0] == "s"))
            {
                SetNewMainWindow(desktop);
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = Services.Provider.GetRequiredService<MainView>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void Desktop_Exit(object sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        TrayIcon.GetIcons(this)?[0]?.Dispose();
        Exit?.Invoke(sender, e);
        await Initializer.StopAsync();
    }

    private async void ExitMenuItem_Click(object sender, EventArgs e)
    {
        await Initializer.StopAsync();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private MainWindow SetNewMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        //由于MainWindow的new需要一定时间，有时候连续调用就会导致重复创建，因此单独建立一个字段来记录
        if (isMainWindowOpened)
        {
            throw new InvalidOperationException("MainWindow已创建");
        }

        isMainWindowOpened = true;
        desktop.MainWindow = Services.Provider.GetRequiredService<MainWindow>();
        desktop.MainWindow.Closed += (s, e) =>
        {
            desktop.MainWindow = null;
            isMainWindowOpened = false;
            Initializer.ClearViewsInstance();
        };
        return desktop.MainWindow as MainWindow;
    }

    private void TrayIcon_Clicked(object sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow is MainWindow m)
            {
                if (m.WindowState == WindowState.Minimized) //最小化
                {
                    m.BringToFront();
                }
                // else //正在显示，直接关窗口
                // {
                //     if (ViewModelBase.Current?.IsWorking ?? false)
                //     {
                //         return;
                //     }
                //
                //     desktop.MainWindow.Close();
                // }
            }
            else //关了窗口，重新开一个新的
            {
                if (!isMainWindowOpened)
                {
                    SetNewMainWindow(desktop).Show();
                }
            }
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }
}