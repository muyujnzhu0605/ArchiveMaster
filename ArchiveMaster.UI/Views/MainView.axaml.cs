using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Dialogs;
using FzLib.Avalonia.Messages;
using ArchiveMaster.Messages;
using ArchiveMaster.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Configs;
using ArchiveMaster.UI;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Platform;

namespace ArchiveMaster.Views;

public partial class MainView : UserControl
{
    private CancellationTokenSource loadingToken = null;

    public MainView()
    {
        InitializeModules();
        AppConfig.Instance.Load();
        InitializeComponent();
        RegisterMessages();
    }
    private static void InitializeModules()
    {
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string[] dllFiles = Directory.GetFiles(currentDirectory, "ArchiveMaster.Module.*.dll");

        foreach (string dllFile in dllFiles)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(dllFile);
                var moduleInitializerTypes = assembly.GetTypes()
                    .Where(t => typeof(IModuleInitializer).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                if (!moduleInitializerTypes.Any())
                {
                    throw new Exception($"在程序集 {dllFile} 中未找到实现 IModuleInitializer 接口的类。");
                }

                foreach (var type in moduleInitializerTypes)
                {
                    IModuleInitializer moduleInitializer = (IModuleInitializer)Activator.CreateInstance(type);
                    moduleInitializer.RegisterConfigs();
                    moduleInitializer.RegisterViews();
                }

                //var s = AssetLoader.Open(new Uri($"avares://{assembly.GetName().Name}/Assets/archive.svg"));

            }
            catch (Exception ex)
            {
                throw new Exception($"加载程序集 {dllFile} 时出错: {ex.Message}");
            }
        }
    }

    private void RegisterMessages()
    {
        this.RegisterDialogHostMessage();
        this.RegisterGetClipboardMessage();
        this.RegisterGetStorageProviderMessage();
        this.RegisterCommonDialogMessage();
        WeakReferenceMessenger.Default.Register<LoadingMessage>(this, (_, m) =>
        {
            if (m.IsVisible)
            {
                loadingToken ??= LoadingOverlay.ShowLoading(this);
            }
            else
            {
                if (loadingToken != null)
                {
                    loadingToken.Cancel();
                    loadingToken = null;
                }
            }
        });
    }
}
