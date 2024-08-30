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
public partial class TimeClassifyViewModel(TimeClassifyConfig config) : TwoStepViewModelBase<TimeClassifyUtility,TimeClassifyConfig>(config)
{
    [ObservableProperty]
    private List<FilesTimeDirInfo> sameTimePhotosDirs;

    protected override Task OnInitializedAsync()
    {
        SameTimePhotosDirs = Utility.TargetDirs;
        return base.OnInitializedAsync();
    }

    protected override void OnReset()
    {
        SameTimePhotosDirs = null;
    }
}
