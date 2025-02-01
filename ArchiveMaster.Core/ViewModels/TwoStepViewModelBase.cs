using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
using ArchiveMaster.Messages;
using ArchiveMaster.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Configs;
using Avalonia.Controls;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ArchiveMaster.ViewModels;

public abstract partial class TwoStepViewModelBase<TService, TConfig> : ViewModelBase
    where TService : TwoStepServiceBase<TConfig>
    where TConfig : ConfigBase, new()
{
    /// <summary>
    /// 能否取消
    /// </summary>
    [ObservableProperty]
    private bool canCancel = false;

    /// <summary>
    /// 是否允许执行
    /// </summary>
    [ObservableProperty]
    private bool canExecute = false;

    /// <summary>
    /// 是否允许初始化
    /// </summary>
    [ObservableProperty]
    private bool canInitialize = true;

    /// <summary>
    /// 是否允许重置
    /// </summary>
    [ObservableProperty]
    private bool canReset = false;

    /// <summary>
    /// 当前配置项
    /// </summary>
    [ObservableProperty]
    private TConfig config;

    /// <summary>
    /// 当前配置版本的名称
    /// </summary>
    [ObservableProperty]
    private string configName;

    /// <summary>
    /// 所有配置项版本的名称
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> configNames;

    [ObservableProperty]
    private string message = "就绪";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressIndeterminate))]
    private double progress;

    protected TwoStepViewModelBase(AppConfig appConfig, string configGroupName)
    {
        ConfigGroupName = configGroupName;
        AppConfig = appConfig;
    }
    protected TwoStepViewModelBase(AppConfig appConfig):this(appConfig,typeof(TConfig).Name)
    {
    }

    /// <summary>
    /// 配置管理
    /// </summary>
    public AppConfig AppConfig { get; }
    
    /// <summary>
    /// 是否启用Two-Step中的初始化。若禁用，将不显示初始化按钮
    /// </summary>
    public virtual bool EnableInitialize => true;
    
    /// <summary>
    /// 当进度为double.NaN时，认为进度为非确定模式
    /// </summary>
    public bool ProgressIndeterminate => double.IsNaN(Progress);
    
    /// <summary>
    /// 配置版本的组名（见AppConfig）
    /// </summary>
    protected string ConfigGroupName { get; }

    /// <summary>
    /// 核心服务
    /// </summary>
    protected TService Service { get; private set; }

    /// <summary>
    /// 进入面板，重置配置和页面
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();
        ConfigNames = new ObservableCollection<string>(AppConfig.GetVersions(ConfigGroupName));
        ConfigName = AppConfig.GetCurrentVersion(ConfigGroupName);
        ResetCommand.Execute(null);
    }

    /// <summary>
    /// 创建服务
    /// </summary>
    protected void CreateService()
    {
        Service = CreateServiceImplement();
        Debug.Assert(Service != null);
        Service.ProgressUpdate += Service_ProgressUpdate;
        Service.MessageUpdate += Service_MessageUpdate;
    }

    /// <summary>
    /// 创建服务实例的具体实现，可以重写
    /// </summary>
    /// <returns></returns>
    protected virtual TService CreateServiceImplement()
    {
        var service = HostServices.GetRequiredService<TService>();
        service.Config = Config;
        return service;
    }

    /// <summary>
    /// 注销服务
    /// </summary>
    private void DisposeService()
    {
        if (Service == null)
        {
            return;
        }

        Service.ProgressUpdate -= Service_ProgressUpdate;
        Service.MessageUpdate -= Service_MessageUpdate;
        Service = null;
    }

    /// <summary>
    /// 执行完成后的任务
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    protected virtual Task OnExecutedAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 执行前的任务
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    protected virtual Task OnExecutingAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 初始化后的任务
    /// </summary>
    /// <returns></returns>
    protected virtual Task OnInitializedAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 初始化前的任务
    /// </summary>
    /// <returns></returns>
    protected virtual Task OnInitializingAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 重置操作
    /// </summary>
    protected virtual void OnReset()
    {
    }

    /// <summary>
    /// 新增配置版本
    /// </summary>
    /// <exception cref="Exception"></exception>
    [RelayCommand]
    private async Task AddConfigAsync()
    {
        if (await this.SendMessage(new InputDialogMessage()
            {
                Type = InputDialogMessage.InputDialogType.Text,
                Title = "新增配置",
                DefaultValue = "新配置",
                Validation = t =>
                {
                    if (string.IsNullOrWhiteSpace(t))
                    {
                        throw new Exception("配置名为空");
                    }

                    if (ConfigNames.Contains(t))
                    {
                        throw new Exception("配置名已存在");
                    }
                }
            }).Task is string result)
        {
            ConfigNames.Add(result);
            AppConfig.GetOrCreateConfig<TConfig>(typeof(TConfig).Name, result);
            ConfigName = result;
        }
    }

    /// <summary>
    /// 取消正在执行或初始化的任务
    /// </summary>
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

    /// <summary>
    /// 建立当前配置的副本
    /// </summary>
    [RelayCommand]
    private void CloneConfig()
    {
        string newName = ConfigName;
        int i = 1;
        while (ConfigNames.Contains(newName))
        {
            i++;
            newName = $"{ConfigName} ({i})";
        }

        var newConfig = AppConfig.GetOrCreateConfig<TConfig>(typeof(TConfig).Name, newName);
        Config.Adapt(newConfig);
        ConfigNames.Add(newName);
        ConfigName = newName;
    }

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="token"></param>
    /// <exception cref="NullReferenceException"></exception>
    [RelayCommand(IncludeCancelCommand = true, CanExecute = nameof(CanExecute))]
    private async Task ExecuteAsync(CancellationToken token)
    {
        if (!EnableInitialize)
        {
            AppConfig.Save(false);
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

    /// <summary>
    /// 初始化任务
    /// </summary>
    /// <param name="token"></param>
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

    /// <summary>
    /// 修改当前配置的版本名称
    /// </summary>
    /// <exception cref="Exception"></exception>
    [RelayCommand]
    private async Task ModifyConfigNameAsync()
    {
        if (await this.SendMessage(new InputDialogMessage()
        {
            Type = InputDialogMessage.InputDialogType.Text,
            Title = "修改配置名称",
            DefaultValue = ConfigName,
            Validation = t =>
            {
                if (string.IsNullOrWhiteSpace(t))
                {
                    throw new Exception("配置名为空");
                }

                if (ConfigNames.Contains(t))
                {
                    throw new Exception("配置名已存在");
                }
            }
        }).Task is string result)
        {
            AppConfig.RenameVersion(ConfigGroupName, ConfigName, result);

            var index = ConfigNames.IndexOf(ConfigName);
            Debug.Assert(index >= 0);
            ConfigNames[index] = result;
            ConfigName = result;
        }
    }

    /// <summary>
    /// 配置版本改变，重新获取该配置版本的配置对象
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    partial void OnConfigNameChanged(string oldValue, string newValue)
    {
        if (newValue == null)
        {
            Config = null;
            return;
        }

        AppConfig.SetCurrentVersion(ConfigGroupName, newValue);
        Config = AppConfig.GetOrCreateConfig<TConfig>(typeof(TConfig).Name, newValue);
        ResetCommand.Execute(null);
    }

    /// <summary>
    /// 移除当前配置
    /// </summary>
    [RelayCommand]
    private async Task RemoveConfigAsync()
    {
        var name = ConfigName;
        var result = await this.SendMessage(new CommonDialogMessage()
        {
            Type = CommonDialogMessage.CommonDialogType.YesNo,
            Title = "删除配置",
            Message = $"是否移除配置：{name}？"
        }).Task;
        if (result.Equals(true))
        {
            ConfigNames.Remove(name);
            AppConfig.RemoveVersion<TConfig>(typeof(TConfig).Name, name);
            if (ConfigNames.Count == 0)
            {
                ConfigNames.Add(AppConfig.DEFAULT_VERSION);
                ConfigName = AppConfig.DEFAULT_VERSION;
            }
            else
            {
                ConfigName = ConfigNames[0];
            }
        }
    }
    
    /// <summary>
    /// 重置
    /// </summary>
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

    private void Service_MessageUpdate(object sender, MessageUpdateEventArgs e)
    {
        Message = e.Message;
    }

    private void Service_ProgressUpdate(object sender, ProgressUpdateEventArgs e)
    {
        Progress = e.Progress;
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
}