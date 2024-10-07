using System.Collections.ObjectModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Models;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels;

public partial class RestoreViewModel : TwoStepViewModelBase<RestoreUtility, BackupTask>
{
    [ObservableProperty]
    private ObservableCollection<BackupTask> tasks;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Config))]
    private BackupTask selectedTask;

    [ObservableProperty]
    private BackupSnapshotEntity selectedSnapshot;

    [ObservableProperty]
    private ObservableCollection<BackupSnapshotEntity> snapshots;

    protected override Task OnInitializingAsync()
    {
        Utility.SnapShotId = SelectedSnapshot?.Id ?? throw new ArgumentNullException("未选择快照");
        return Task.CompletedTask;
    }

    async partial void OnSelectedTaskChanged(BackupTask value)
    {
        var utility = CreateUtilityImplement();
        Snapshots = new ObservableCollection<BackupSnapshotEntity>(await utility.GetSnapshotsAsync(default));
        if (Snapshots.Count > 0)
        {
            SelectedSnapshot = Snapshots[^1];
        }
    }

    protected override RestoreUtility CreateUtilityImplement()
    {
        return new RestoreUtility(SelectedTask, appConfig);
    }

    public override BackupTask Config => SelectedTask;

    public RestoreViewModel(AppConfig appConfig) : base(null, appConfig)
    {
    }

    public override void OnEnter()
    {
        Tasks = new ObservableCollection<BackupTask>(Services.Provider.GetRequiredService<FileBackupperConfig>().Tasks);
        if (SelectedTask == null && Tasks.Count > 0)
        {
            SelectedTask = Tasks[0];
        }
    }
}