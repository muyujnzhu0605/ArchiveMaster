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

namespace ArchiveMaster.ViewModels;
public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
    }
    [ObservableProperty]
    private object mainContent;

    public List<ToolPanelGroupInfo> PanelGroups { get; } = ToolPanelInfo.Groups;

    [ObservableProperty]
    private bool isToolOpened;

    [RelayCommand]
    private void EnterTool(ToolPanelInfo panelInfo)
    {
        if (panelInfo.PanelInstance == null)
        {
            panelInfo.PanelInstance = Activator.CreateInstance(panelInfo.PanelType) as TwoStepPanelBase;
            (panelInfo.PanelInstance as TwoStepPanelBase).RequestClosing += (s, e) =>
            {
                IsToolOpened = false;
            };
        }
        TwoStepPanelBase panel = panelInfo.PanelInstance as TwoStepPanelBase;
        panel.Title = panelInfo.Title;
        panel.Description = panelInfo.Description;
        MainContent = panel;
        IsToolOpened = true;
    }
}
