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

public partial class UselessJpgCleanerViewModel : TwoStepViewModelBase
{
    private UselessJpgCleanerUtility utility;

    public UselessJpgCleanerConfig Config { get; set; } = AppConfig.Instance.Get<UselessJpgCleanerConfig>();

    [ObservableProperty]
    private string dir;

    [ObservableProperty]
    private List<SimpleFileInfo> deletingJpgFiles;

    protected override async Task InitializeImplAsync()
    {
        Config.Dir = Dir;
        utility = new UselessJpgCleanerUtility(Config);
        utility.ProgressUpdate += Utility_ProgressUpdate;
        await utility.InitializeAsync();
        DeletingJpgFiles = utility.DeletingJpgFiles;
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        await utility.ExecuteAsync(token);
        utility.ProgressUpdate -= Utility_ProgressUpdate;
        utility = null;
        DeletingJpgFiles = null;
    }

    protected override void ResetImpl()
    {
        utility = null;
        DeletingJpgFiles = null;
    }
}
