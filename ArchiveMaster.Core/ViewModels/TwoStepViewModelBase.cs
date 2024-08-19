using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
using ArchiveMaster.Messages;
using ArchiveMaster.Utilities;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Configs;
using Avalonia.Controls;
using Serilog;

namespace ArchiveMaster.ViewModels;

public abstract partial class TwoStepViewModelBase<TUtility, TConfig> : ViewModelBase<TUtility, TConfig>
    where TUtility : TwoStepUtilityBase<TConfig>
    where TConfig : ConfigBase
{
    public TwoStepViewModelBase(TUtility utility, TConfig config, bool enableInitialize = true) : base(utility, config)
    {
        EnableInitialize = enableInitialize;
        CanInitialize = enableInitialize;
        CanExecute = !enableInitialize;
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

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ProgressIndeterminate))]
    private double progress;

    public bool EnableInitialize { get; }

    public bool ProgressIndeterminate => double.IsNaN(Progress);

    protected override TUtility CreateUtility()
    {
        var utility = base.CreateUtility();
        Utility.ProgressUpdate += Utility_ProgressUpdate;
        Utility.MessageUpdate += Utility_MessageUpdate;
        return utility;
    }

    protected override void DisposeUtility()
    {
        if (Utility == null)
        {
            return;
        }

        Utility.ProgressUpdate -= Utility_ProgressUpdate;
        Utility.MessageUpdate -= Utility_MessageUpdate;
        base.DisposeUtility();
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
            CreateUtility();
        }

        if (Utility == null)
        {
            throw new NullReferenceException($"{nameof(Utility)}为空");
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
            await Utility.ExecuteAsync(token);
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
        AppConfig.Instance.Save(false);
        CanInitialize = false;
        InitializeCommand.NotifyCanExecuteChanged();
        CanReset = false;
        ResetCommand.NotifyCanExecuteChanged();
        CanCancel = true;
        CancelCommand.NotifyCanExecuteChanged();

        if (await TryRunAsync(async () =>
            {
                var u = CreateUtility();
                await OnInitializingAsync();
                Config.Check();
                await u.InitializeAsync(token);
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

        OnReset();
        DisposeUtility();
    }

    private async Task<bool> TryRunAsync(Func<Task> action, string errorTitle)
    {
        Progress = double.NaN;
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

    private void Utility_ProgressUpdate(object sender, ProgressUpdateEventArgs e)
    {
        Progress = e.Progress;
    }

    private void Utility_MessageUpdate(object sender, MessageUpdateEventArgs e)
    {
        Message = e.Message;
    }
}