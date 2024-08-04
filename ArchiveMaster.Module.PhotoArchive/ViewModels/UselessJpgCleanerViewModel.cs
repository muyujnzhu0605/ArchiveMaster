using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveMaster.ViewModels;

public partial class UselessJpgCleanerViewModel : TwoStepViewModelBase<UselessJpgCleanerUtility>
{
    public override UselessJpgCleanerConfig Config { get; } = AppConfig.Instance.Get<UselessJpgCleanerConfig>();

    [ObservableProperty]
    private List<SimpleFileInfo> deletingJpgFiles;

    protected override Task OnInitializedAsync()
    {
        DeletingJpgFiles = Utility.DeletingJpgFiles;
        return base.OnInitializedAsync();
    }

    protected override void OnReset()
    {
        DeletingJpgFiles = null;
    }
}