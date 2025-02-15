using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ArchiveMaster.Configs;
using ArchiveMaster.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Enums;
using ArchiveMaster.ViewModels.FileSystem;

namespace ArchiveMaster.ViewModels;

public partial class BatchCommandLineViewModel : TwoStepViewModelBase<BatchCommandLineService, BatchCommandLineConfig>
{
    [ObservableProperty]
    private bool showLevels;

    [ObservableProperty]
    private List<BatchCommandLineFileInfo> files;

    /// <inheritdoc/>
    public BatchCommandLineViewModel(AppConfig appConfig) : base(appConfig)
    {
    }

    protected override void OnConfigChanged()
    {
        Config.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(BatchCommandLineConfig.Target))
            {
                SetLevelsVisibility();
            }
        };
        SetLevelsVisibility();
    }

    private void SetLevelsVisibility()
    {
        ShowLevels = Config.Target is BatchTarget.SpecialLevelDirs or BatchTarget.SpecialLevelElements
            or BatchTarget.SpecialLevelFiles;
    }

    protected override Task OnInitializedAsync()
    {
        Files = Service.Files;
        return base.OnInitializedAsync();
    }

    protected override void OnReset()
    {
        Files = null;
    }

    [RelayCommand]
    private void SetProcess(string p)
    {
        if (p.Contains(' '))
        {
            var parts = p.Split(' ', 2);
            Config.Program = parts[0];
            Config.Arguments = parts[1];
        }
        else
        {
            Config.Program = p;
        }
    }
}