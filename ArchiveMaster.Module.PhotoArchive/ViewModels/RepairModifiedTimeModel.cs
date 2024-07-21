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

public partial class RepairModifiedTimeViewModel : TwoStepViewModelBase
{
    private RepairModifiedTimeUtility utility;

    public RepairModifiedTimeConfig Config { get; set; } = AppConfig.Instance.Get<RepairModifiedTimeConfig>();

    [ObservableProperty]
    private string dir;

    [ObservableProperty]
    private List<string> updatingFiles;

    [ObservableProperty]
    private List<string> errorFiles;

    protected override async Task InitializeImplAsync()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Dir);
        Config.Dir = Dir;
        utility = new RepairModifiedTimeUtility(Config);
        utility.ProgressUpdate += Utility_ProgressUpdate;
        await utility.InitializeAsync();
        UpdatingFiles = utility.UpdatingFilesAndMessages;
        ErrorFiles = utility.ErrorFilesAndMessages;
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(utility, nameof(utility));
        await utility.ExecuteAsync(token);
        UpdatingFiles = null;
        ErrorFiles = utility.ErrorFilesAndMessages;
        utility.ProgressUpdate -= Utility_ProgressUpdate;
        utility = null;
        Message = "完成";
    }

    protected override void ResetImpl()
    {
        UpdatingFiles = new List<string>();
        ErrorFiles = new List<string>();
    }
}
