using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveMaster.ViewModels;

public partial class RenameViewModel(RenameConfig config)
    : TwoStepViewModelBase<RenameUtility, RenameConfig>(config)
{
    [ObservableProperty]
    private ObservableCollection<RenameFileInfo> files;

    [ObservableProperty]
    private bool showMatchedOnly = true;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private int matchedCount;

    protected override Task OnInitializedAsync()
    {
        var matched = Utility.Files.Where(p => p.IsMatched);
        Files = new ObservableCollection<RenameFileInfo>(ShowMatchedOnly ? matched : Utility.Files);
        TotalCount = Utility.Files.Count;
        MatchedCount = matched.Count();
        return base.OnInitializedAsync();
    }

    partial void OnShowMatchedOnlyChanged(bool value)
    {
        if (Utility?.Files == null)
        {
            return;
        }

        Files = new ObservableCollection<RenameFileInfo>(value ? Utility.Files.Where(p => p.IsMatched) : Utility.Files);
    }

    protected override void OnReset()
    {
        Files = null;
        TotalCount = 0;
        MatchedCount = 0;
    }
}