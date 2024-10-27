using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ArchiveMaster.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static ArchiveMaster.ViewModels.MainViewModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Platforms;
using ArchiveMaster.Utilities;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IStartupManager startupManager;

    [ObservableProperty]
    private bool isAutoStart;

    [ObservableProperty]
    private bool isToolOpened;

    [ObservableProperty]
    private object mainContent;

    [ObservableProperty]
    private ObservableCollection<ToolPanelGroupInfo> panelGroups = new ObservableCollection<ToolPanelGroupInfo>();

    public MainViewModel(AppConfig appConfig, IStartupManager startupManager,
        IBackCommandService backCommandService = null)
    {
        this.startupManager = startupManager;
        foreach (var view in Initializer.Views)
        {
            PanelGroups.Add(view);
        }

        backCommandService?.RegisterBackCommand(() =>
        {
            if (mainContent is PanelBase && IsToolOpened)
            {
                IsToolOpened = false;
                return true;
            }

            return false;
        });
        BackCommandService = backCommandService;

        IsAutoStart = startupManager.IsStartupEnabled();
    }

    public IBackCommandService BackCommandService { get; }

    [RelayCommand]
    private void EnterTool(ToolPanelInfo panelInfo)
    {
        if (panelInfo.PanelInstance == null)
        {
            panelInfo.PanelInstance = Services.Provider.GetService(panelInfo.PanelType) as PanelBase ??
                                      throw new Exception($"无法找到{panelInfo.PanelType}服务");
            if (panelInfo.PanelInstance.DataContext is ViewModelBase vm)
            {
                vm.RequestClosing += async (s, e) =>
                {
                    CancelEventArgs args = new CancelEventArgs();
                    if ((s as StyledElement)?.DataContext is ViewModelBase vm)
                    {
                        await vm.OnExitAsync(args);
                    }

                    if (!args.Cancel)
                    {
                        IsToolOpened = false;
                    }
                };
            }

            panelInfo.PanelInstance.Title = panelInfo.Title;
            panelInfo.PanelInstance.Description = panelInfo.Description;
        }

        (panelInfo.PanelInstance.DataContext as ViewModelBase)?.OnEnter();
        MainContent = panelInfo.PanelInstance;
        IsToolOpened = true;
    }

    [RelayCommand]
    private void SetAutoStart(bool autoStart)
    {
        if (autoStart)
        {
            startupManager.EnableStartup("s");
        }
        else
        {
            startupManager.DisableStartup();
        }
    }
}