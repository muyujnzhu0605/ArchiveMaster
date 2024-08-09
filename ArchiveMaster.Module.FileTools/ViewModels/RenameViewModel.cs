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

public partial class RenameViewModel : TwoStepViewModelBase<RenameUtility>
{
    [ObservableProperty]
    private ObservableCollection<RenameFileInfo> files;

    public override RenameConfig Config { get; } = AppConfig.Instance.Get<RenameConfig>();

    protected override Task OnInitializedAsync()
    {
        Files = new ObservableCollection<RenameFileInfo>(Utility.Files);
        return base.OnInitializedAsync();
    }
}