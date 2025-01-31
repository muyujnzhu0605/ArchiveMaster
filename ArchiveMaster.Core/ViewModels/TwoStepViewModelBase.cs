using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
using ArchiveMaster.Messages;
using ArchiveMaster.Services;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Configs;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ArchiveMaster.ViewModels;

public abstract partial class TwoStepViewModelBase<TService, TConfig> : ViewModelBase
    where TService : TwoStepServiceBase<TConfig>
    where TConfig : ConfigBase, new()
{
    protected TwoStepViewModelBase()
    {
    }

    protected TwoStepViewModelBase(TConfig config, AppConfig appConfig)
    {
        Config = config;
        AppConfig = appConfig;
    }

    protected TService Service { get; private set; }

    [ObservableProperty]
    private TConfig config;

    protected virtual TService CreateServiceImplement()
    {
        var service = HostServices.GetRequiredService<TService>();
        service.Config = Config;
        return service;
    }

    [ObservableProperty]
    private bool canExecute = false;

    [ObservableProperty]
    private bool canInitialize = true;

    [ObservableProperty]
    private bool canReset = false;

    [ObservableProperty]
    private bool canCancel = false;

    [ObservableProperty]
    private string message = "就绪";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressIndeterminate))]
    private double progress;

    public AppConfig AppConfig { get; protected internal init; }

    public virtual bool EnableInitialize => true;

    public bool ProgressIndeterminate => double.IsNaN(Progress);

    protected void CreateService()
    {
        Service = CreateServiceImplement();
        Debug.Assert(Service != null);
        Service.ProgressUpdate += Service_ProgressUpdate;
        Service.MessageUpdate += Service_MessageUpdate;
    }

    protected void DisposeService()
    {
        if (Service == null)
        {
            return;
        }

        Service.ProgressUpdate -= Service_ProgressUpdate;
        Service.MessageUpdate -= Service_MessageUpdate;
        Service = null;
    }

    protected virtual Task OnExecutedAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnExecutingAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnInitializedAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnInitializingAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual void OnReset()
    {
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        WeakReferenceMessenger.Default.Send(new LoadingMessage(true));
        if (InitializeCommand.IsRunning)
        {
            InitializeCommand.Cancel();
            CanInitialize = false;
            InitializeCommand.NotifyCanExecuteChanged();
        }
        else if (ExecuteCommand.IsRunning)
        {
            ExecuteCommand.Cancel();
            CanExecute = false;
            ExecuteCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(IncludeCancelCommand = true, CanExecute = nameof(CanExecute))]
    private async Task ExecuteAsync(CancellationToken token)
    {
        if (!EnableInitialize)
        {
            CreateService();
        }

        if (Service == null)
        {
            throw new NullReferenceException($"{nameof(Service)}为空");
        }

        CanExecute = false;
        CanReset = false;
        ResetCommand.NotifyCanExecuteChanged();
        CanCancel = true;
        CancelCommand.NotifyCanExecuteChanged();

        await TryRunAsync(async () =>
        {
            await OnExecutingAsync(token);
            Config.Check();
            await Service.ExecuteAsync(token);
            await OnExecutedAsync(token);
        }, "执行失败");

        CanReset = true;
        ResetCommand.NotifyCanExecuteChanged();
        CanCancel = false;
        CancelCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(IncludeCancelCommand = true, CanExecute = nameof(CanInitialize))]
    private async Task InitializeAsync(CancellationToken token)
    {
        Stopwatch sw = Stopwatch.StartNew();
        AppConfig.Save(false);
        var a = sw.ElapsedMilliseconds;
        CanInitialize = false;
        InitializeCommand.NotifyCanExecuteChanged();
        var b = sw.ElapsedMilliseconds;
        CanReset = false;
        ResetCommand.NotifyCanExecuteChanged();
        CanCancel = true;
        CancelCommand.NotifyCanExecuteChanged();
        var c = sw.ElapsedMilliseconds;

        if (await TryRunAsync(async () =>
            {
                var d = sw.ElapsedMilliseconds;
                CreateService();
                var e = sw.ElapsedMilliseconds;
                await OnInitializingAsync();
                Config.Check();
                await Service.InitializeAsync(token);
                await OnInitializedAsync();
            }, "初始化失败"))
        {
            CanExecute = true;
            CanReset = true;
        }
        else
        {
            CanExecute = false;
            CanReset = false;
            CanInitialize = true;
        }

        ExecuteCommand.NotifyCanExecuteChanged();
        ResetCommand.NotifyCanExecuteChanged();
        InitializeCommand.NotifyCanExecuteChanged();
        CanCancel = false;
        CancelCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
    {
        CanReset = false;
        CanInitialize = EnableInitialize;
        CanExecute = !EnableInitialize;

        ResetCommand.NotifyCanExecuteChanged();
        ExecuteCommand.NotifyCanExecuteChanged();
        InitializeCommand.NotifyCanExecuteChanged();

        Message = "就绪";
        OnReset();
        DisposeService();
    }

    private async Task<bool> TryRunAsync(Func<Task> action, string errorTitle)
    {
        Progress = double.NaN;
        Message = "正在处理";
        IsWorking = true;
        try
        {
            await action();
            return true;
        }
        catch (OperationCanceledException ex)
        {
            WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Ok,
                Title = "操作已取消",
                Message = "操作已取消",
                Detail = ex.ToString()
            });
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "执行工具失败");
            WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Error,
                Title = errorTitle,
                Exception = ex
            });
            return false;
        }
        finally
        {
            Progress = 0;
            IsWorking = false;
            Message = "完成";
            WeakReferenceMessenger.Default.Send(new LoadingMessage(false));
        }
    }

    private void Service_ProgressUpdate(object sender, ProgressUpdateEventArgs e)
    {
        Progress = e.Progress;
    }

    private void Service_MessageUpdate(object sender, MessageUpdateEventArgs e)
    {
        Message = e.Message;
    }
}