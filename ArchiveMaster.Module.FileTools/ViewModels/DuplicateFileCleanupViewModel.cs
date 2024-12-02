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
using ArchiveMaster.Basic;
using ArchiveMaster.ViewModels.FileSystem;

namespace ArchiveMaster.ViewModels;

public partial class DuplicateFileCleanupViewModel(DuplicateFileCleanupConfig config, AppConfig appConfig)
    : TwoStepViewModelBase<DuplicateFileCleanupService, DuplicateFileCleanupConfig>(config, appConfig)
{
    [ObservableProperty]
    private ObservableCollection<DuplicateFileInfo> files;

    [ObservableProperty]
    private BulkObservableCollection<SimpleFileInfo> groups;

    protected override Task OnInitializedAsync()
    {
        Files = new ObservableCollection<DuplicateFileInfo>(Service.DuplicateFiles);
        Groups = new BulkObservableCollection<SimpleFileInfo>(Service.DuplicateGroups.SubDirs);
        return base.OnInitializedAsync();
    }

    protected override void OnReset()
    {
        Files = null;
        Groups = null;
    }
}