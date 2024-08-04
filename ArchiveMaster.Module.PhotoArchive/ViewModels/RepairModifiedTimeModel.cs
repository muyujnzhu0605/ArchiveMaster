using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Cryptography;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveMaster.ViewModels;

public partial class RepairModifiedTimeViewModel : TwoStepViewModelBase<RepairModifiedTimeUtility>
{
    public override RepairModifiedTimeConfig Config { get; } = AppConfig.Instance.Get<RepairModifiedTimeConfig>();

    [ObservableProperty]
    private List<string> updatingFiles;

    [ObservableProperty]
    private List<string> errorFiles;

    protected override Task OnInitializedAsync()
    {
        UpdatingFiles = Utility.UpdatingFilesAndMessages;
        ErrorFiles = Utility.ErrorFilesAndMessages;
        return base.OnInitializedAsync();
    }

    protected override void OnReset()
    {
        UpdatingFiles = new List<string>();
        ErrorFiles = new List<string>();
    }
}