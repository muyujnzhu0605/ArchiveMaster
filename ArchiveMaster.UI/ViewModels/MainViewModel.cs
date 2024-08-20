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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static ArchiveMaster.ViewModels.MainViewModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Platforms;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(Initializer initializer, IBackCommandService backCommandService = null)
    {
        foreach (var view in initializer.Views)
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
    }

    [ObservableProperty]
    private object mainContent;

    [ObservableProperty]
    private ObservableCollection<ToolPanelGroupInfo> panelGroups = new ObservableCollection<ToolPanelGroupInfo>();

    [ObservableProperty]
    private bool isToolOpened;

    [RelayCommand]
    private void EnterTool(ToolPanelInfo panelInfo)
    {
        if (panelInfo.PanelInstance == null)
        {
            panelInfo.PanelInstance = Services.Provider.GetService(panelInfo.PanelType) as PanelBase ??
                                      throw new Exception($"无法找到{panelInfo.PanelType}服务");
            panelInfo.PanelInstance.RequestClosing += (s, e) => { IsToolOpened = false; };
            panelInfo.PanelInstance.Title = panelInfo.Title;
            panelInfo.PanelInstance.Description = panelInfo.Description;
        }

        (panelInfo.PanelInstance.DataContext as ViewModelBase)?.OnEnter();
        MainContent = panelInfo.PanelInstance;
        IsToolOpened = true;
    }
}