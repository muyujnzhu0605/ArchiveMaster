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

public partial class RepairModifiedTimeViewModel(RepairModifiedTimeConfig config, AppConfig appConfig)
    : TwoStepViewModelBase<RepairModifiedTimeUtility, RepairModifiedTimeConfig>(config, appConfig)
{
    [ObservableProperty]
    private List<ExifTimeFileInfo> files = new List<ExifTimeFileInfo>();

    protected override Task OnInitializedAsync()
    {
        Files = Utility.Files.ToList();
        return base.OnInitializedAsync();
    }

    protected override void OnReset()
    {
        Files = new List<ExifTimeFileInfo>();
    }
}