using System.Collections.ObjectModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib.Avalonia.Messages;

namespace ArchiveMaster.ViewModels;

public abstract partial class
    MultiVersionConfigTwoStepViewModelBase<TService, TConfig> : TwoStepViewModelBase<TService, TConfig>
    where TService : TwoStepServiceBase<TConfig>
    where TConfig : ConfigBase, new()
{
    protected string ConfigGroupName { get; }

    protected MultiVersionConfigTwoStepViewModelBase(AppConfig appConfig, string configGroupName)
    {
        ConfigGroupName = configGroupName;
        AppConfig = appConfig;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        ConfigNames = new ObservableCollection<string>(AppConfig.GetVersions(ConfigGroupName));
        ConfigName = AppConfig.GetCurrentVersion(ConfigGroupName);
    }

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
            AppConfig.AddVersion<TConfig>(typeof(TConfig).Name, result);
            ConfigName = result;
        }
    }

    partial void OnConfigNameChanged(string oldValue, string newValue)
    {
        AppConfig.SetCurrentVersion(ConfigGroupName, newValue);
        Config = AppConfig.GetConfig<TConfig>(typeof(TConfig).Name, newValue);
        ResetCommand.Execute(null);
    }


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


    [ObservableProperty]
    private string configName;

    [ObservableProperty]
    private ObservableCollection<string> configNames;
}