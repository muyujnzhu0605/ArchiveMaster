using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
using ArchiveMaster.Configs;
using ArchiveMaster.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveMaster.ViewModels;

public partial class RenameViewModel(RenameConfig config, AppConfig appConfig)
    : TwoStepViewModelBase<RenameService, RenameConfig>(config, appConfig)
{
    [ObservableProperty]
    private ObservableCollection<FileSystem.RenameFileInfo> files;

    [ObservableProperty]
    private bool showMatchedOnly = true;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private int matchedCount;

    protected override Task OnInitializedAsync()
    {
        var matched = Service.Files.Where(p => p.IsMatched);
        Files = new ObservableCollection<FileSystem.RenameFileInfo>(ShowMatchedOnly ? matched : Service.Files);
        TotalCount = Service.Files.Count;
        MatchedCount = matched.Count();
        return base.OnInitializedAsync();
    }

    partial void OnShowMatchedOnlyChanged(bool value)
    {
        if (Service?.Files == null)
        {
            return;
        }

        Files = new ObservableCollection<FileSystem.RenameFileInfo>(value ? Service.Files.Where(p => p.IsMatched) : Service.Files);
    }

    protected override void OnReset()
    {
        Files = null;
        TotalCount = 0;
        MatchedCount = 0;
    }
}