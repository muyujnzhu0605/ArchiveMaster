using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
using FzLib.Cryptography;
using Mapster;
using ArchiveMaster.Configs;
using ArchiveMaster.Services;
using ArchiveMaster.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.ViewModels.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels;

public partial class PhotoSlimmingViewModel : TwoStepViewModelBase<PhotoSlimmingService, PhotoSlimmingConfig>
{
    [ObservableProperty]
    private bool canCancel;

    [ObservableProperty]
    private SlimmingFilesInfo compressFiles;

    [ObservableProperty]
    private PhotoSlimmingConfig selectedConfig;

    [ObservableProperty]
    private SlimmingFilesInfo copyFiles;

    [ObservableProperty]
    private SlimmingFilesInfo deleteFiles;

    [ObservableProperty]
    private ObservableCollection<string> errorMessages;
    private readonly AppConfig appConfig;

    public override PhotoSlimmingConfig Config => SelectedConfig;

    protected override PhotoSlimmingService CreateServiceImplement()
    {
        return new PhotoSlimmingService(Config, appConfig);
    }

    public PhotoSlimmingViewModel(AppConfig appConfig, PhotoSlimmingConfig config = null)
        : base(config, appConfig)
    {
        Configs = HostServices.Provider.GetRequiredService<PhotoSlimmingCollectionConfig>().List;
        if (Configs.Count == 0)
        {
            Configs.Add(new PhotoSlimmingConfig());
        }

        SelectedConfig = Configs[0];
        this.appConfig = appConfig;
    }

    public ObservableCollection<PhotoSlimmingConfig> Configs { get; set; }

    protected override Task OnExecutedAsync(CancellationToken token)
    {
        ErrorMessages = new ObservableCollection<string>(Service.ErrorMessages);
        return base.OnExecutedAsync(token);
    }

    protected override Task OnInitializingAsync()
    {
        if (Config == null)
        {
            throw new ArgumentException("请先选择配置");
        }

        return base.OnInitializingAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        Progress = -1;
        Message = "正在生成统计信息";
        Progress = Double.NaN;
        await Service.CopyFiles.CreateRelativePathsAsync();
        await Service.CompressFiles.CreateRelativePathsAsync();
        await Service.DeleteFiles.CreateRelativePathsAsync();
        CopyFiles = Service.CopyFiles;
        CompressFiles = Service.CompressFiles;
        DeleteFiles = Service.DeleteFiles;
        ErrorMessages = new ObservableCollection<string>(Service.ErrorMessages);
    }

    protected override void OnReset()
    {
        CopyFiles = null;
        CompressFiles = null;
        DeleteFiles = null;
        ErrorMessages = null;
    }

    [RelayCommand]
    private void Clone()
    {
        var newObj = Config.Adapt<PhotoSlimmingConfig>();
        newObj.Name += "（复制）";
        Configs.Add(newObj);
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        var message = new DialogHostMessage(new PhotoSlimmingConfigDialog());
        WeakReferenceMessenger.Default.Send(message);
        var result = await message.Task;
        if (result is PhotoSlimmingConfig config)
        {
            Configs.Add(config);
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        var message = new DialogHostMessage(new PhotoSlimmingConfigDialog(SelectedConfig));
        WeakReferenceMessenger.Default.Send(message);
        var result = await message.Task;
        if (result is PhotoSlimmingConfig config)
        {
            Configs[Configs.IndexOf(SelectedConfig)] = config;
            SelectedConfig = config;
        }
    }

    [RelayCommand]
    private void Remove()
    {
        Configs.Remove(SelectedConfig);
    }
}