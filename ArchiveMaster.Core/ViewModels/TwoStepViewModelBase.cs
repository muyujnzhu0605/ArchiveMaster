using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
using ArchiveMaster.Messages;
using ArchiveMaster.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveMaster.ViewModels;

public abstract partial class TwoStepViewModelBase<TUtility> : ViewModelBase where TUtility : TwoStepUtilityBase
{
    [ObservableProperty]
    private bool canExecute = false;

    [ObservableProperty]
    private bool canInitialize = true;

    [ObservableProperty]
    private bool canReset = false;

    [ObservableProperty]
    private bool isEnable = true;

    [ObservableProperty]
    private bool isWorking = false;

    [ObservableProperty]
    private string message = "就绪";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressIndeterminate))]
    private double progress;

    public TwoStepViewModelBase(bool enableInitialize)
    {
        EnableInitialize = enableInitialize;
        CanInitialize = enableInitialize;
        CanExecute = !enableInitialize;
    }

    public TwoStepViewModelBase() : this(true)
    {
    }
    public bool EnableInitialize { get; }

    public bool ProgressIndeterminate => Progress < 0;
    protected override TUtility Utility => base.Utility as TUtility;
    protected override T CreateUtility<T>()
    {
        var utility = base.CreateUtility<T>();
        Utility.ProgressUpdate += Utility_ProgressUpdate;
        return utility;
    }

    protected override void DisposeUtility()
    {
        if (Utility == null)
        {
            return;
        }
        Utility.ProgressUpdate -= Utility_ProgressUpdate;
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
    [RelayCommand]
    private void CancelExecute()
    {
        WeakReferenceMessenger.Default.Send(new LoadingMessage(true));
        ExecuteCommand.Cancel();
        CanExecute = false;
        ExecuteCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(IncludeCancelCommand = true, CanExecute = nameof(CanExecute))]
    private async Task ExecuteAsync(CancellationToken token)
    {
        if (!EnableInitialize)
        {
            CreateUtility<TUtility>();
        }

        if (Utility == null)
        {
            throw new NullReferenceException($"{nameof(Utility)}为空");
        }

        if (Utility is not TwoStepUtilityBase t)
        {
            throw new ArgumentException($"{nameof(Utility)}不是{nameof(TwoStepUtilityBase)}的子类");
        }


        CanExecute = false;

        await TryRunAsync(async () =>
        {
            await OnExecutingAsync(token);
            Config.Check();
            await t.ExecuteAsync(token);
            await OnExecutedAsync(token);
        }, "执行失败");

        CanReset = true;
        ResetCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanInitialize))]
    private async Task InitializeAsync()
    {
        CanInitialize = false;
        InitializeCommand.NotifyCanExecuteChanged();
        CanReset = false;
        ResetCommand.NotifyCanExecuteChanged();

        if (await TryRunAsync(async () =>
            {
                var u = CreateUtility<TUtility>();
                await OnInitializingAsync();
                Config.Check();
                await u.InitializeAsync();
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
        Progress = -1;
        IsWorking = true;
        try
        {
            await action();
            return true;
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
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

    private void Utility_ProgressUpdate(object sender, ProgressUpdateEventArgs<int> e)
    {
        Progress = 1.0 * e.Current / e.Maximum;
        Message = e.Message;
    }
}