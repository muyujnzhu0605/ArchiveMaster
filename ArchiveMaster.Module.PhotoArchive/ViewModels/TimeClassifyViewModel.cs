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
public partial class TimeClassifyViewModel : TwoStepViewModelBase
{
    private TimeClassifyUtility utility;

    public TimeClassifyConfig Config { get; set; } = AppConfig.Instance.Get(nameof(TimeClassifyConfig)) as TimeClassifyConfig;

    [ObservableProperty]
    private string dir;

    [ObservableProperty]
    private List<SimpleDirInfo> sameTimePhotosDirs;

    protected override async Task InitializeImplAsync()
    {
        Config.Dir = Dir;
        utility = new TimeClassifyUtility(Config);
        utility.ProgressUpdate += Utility_ProgressUpdate;
        await utility.InitializeAsync();
        SameTimePhotosDirs = utility.TargetDirs;
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(utility);
        await utility.ExecuteAsync(token);
        utility.ProgressUpdate -= Utility_ProgressUpdate;
        utility = null;
        SameTimePhotosDirs = null;
    }

    protected override void ResetImpl()
    {
        SameTimePhotosDirs = null;
    }
}
