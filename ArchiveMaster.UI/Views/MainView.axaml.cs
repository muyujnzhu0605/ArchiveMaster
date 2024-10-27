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
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Interactivity;
using ArchiveMaster.Platforms;
using FzLib;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Avalonia.Threading;
using Serilog;

namespace ArchiveMaster.Views;

public partial class MainView : UserControl
{
    private readonly AppConfig appConfig;
    private readonly IPermissionService permissionService;
    private CancellationTokenSource loadingToken = null;

    public MainView(MainViewModel viewModel,
        AppConfig appConfig,
        IViewPadding viewPadding = null,
        IPermissionService permissionService = null)
    {
        this.appConfig = appConfig;
        this.permissionService = permissionService;
        DataContext = viewModel;

        InitializeComponent();
        RegisterMessages();
        if (viewPadding != null)
        {
            Padding = new Thickness(0, viewPadding.GetTop(), 0, viewPadding.GetBottom());
        }

        Initializer.ModuleInitializers.ForEach(p => p.RegisterMessages(this));
    }

    private void RegisterMessages()
    {
        this.RegisterDialogHostMessage();
        this.RegisterGetClipboardMessage();
        this.RegisterGetStorageProviderMessage();
        this.RegisterCommonDialogMessage();
        WeakReferenceMessenger.Default.Register<LoadingMessage>(this, (o, m) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (m.IsVisible && o is Visual v)
                {
                    try
                    {
                        loadingToken ??= LoadingOverlay.ShowLoading(v);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Loading打开失败");
                    }
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
        });
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        permissionService?.CheckPermissions();
        if (appConfig.LoadError != null)
        {
            await this.ShowErrorDialogAsync("加载配置失败", appConfig.LoadError);
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (Bounds.Width <= 420)
        {
            Resources["BoxWidth"] = 160d;
            Resources["BoxHeight"] = 200d;
            Resources["ShowDescription"] = false;
        }
        else
        {
            Resources["BoxWidth"] = 200d;
            Resources["BoxHeight"] = 280d;
            Resources["ShowDescription"] = true;
        }
    }

    private void ToolItem_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            TopLevel.GetTopLevel(this).FocusManager.ClearFocus();
            (DataContext as MainViewModel).EnterToolCommand.Execute((sender as ToolItemBox).DataContext);
        }
    }
}