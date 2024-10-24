using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Svg.Skia;
using Serilog;

namespace ArchiveMaster.UI.Desktop;

class Program
{
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        GC.KeepAlive(typeof(SvgImageExtension).Assembly);
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);
        return AppBuilder.Configure<App>()
            .With(new X11PlatformOptions()
            {
                UseDBusFilePicker = false,
            })
            .UsePlatformDetect()
            .LogToTrace();
    }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/logs.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        Log.Information("程序启动");
#if !DEBUG
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Log.Fatal(e.Exception, "未捕获的Task错误");
            Log.CloseAndFlush();
        };
        try
        {
#endif
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
#if !DEBUG
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "未捕获的主线程错误");
        }
        finally
        {
            Log.CloseAndFlush();
        }
#endif
    }
}