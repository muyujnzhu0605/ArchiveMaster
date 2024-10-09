using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Models;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace ArchiveMaster.ViewModels;

public partial class RestoreViewModel : TwoStepViewModelBase<RestoreUtility, BackupTask>
{
    [ObservableProperty]
    private bool isSnapshotComboBoxEnable;

    [ObservableProperty]
    private BackupSnapshotEntity selectedSnapshot;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Config))]
    private BackupTask selectedTask;

    [ObservableProperty]
    private ObservableCollection<BackupSnapshotEntity> snapshots;

    [ObservableProperty]
    private ObservableCollection<BackupTask> tasks;

    [ObservableProperty]
    private BulkObservableCollection<SimpleFileInfo> treeFiles;

    public RestoreViewModel(AppConfig appConfig) : base(null, appConfig)
    {
    }

    public override BackupTask Config => SelectedTask;

    public override void OnEnter()
    {
        Tasks = new ObservableCollection<BackupTask>(Services.Provider.GetRequiredService<FileBackupperConfig>().Tasks);
        if (SelectedTask == null && Tasks.Count > 0)
        {
            SelectedTask = Tasks[0];
        }
    }

    protected override RestoreUtility CreateUtilityImplement()
    {
        return new RestoreUtility(SelectedTask, appConfig);
    }

    protected override Task OnInitializedAsync()
    {
        Utility.RootDir.Reorder();
        var files = new BulkObservableCollection<SimpleFileInfo>();
        files.AddRange(Utility.RootDir.Subs);
        TreeFiles = files;
        return base.OnInitializedAsync();
    }

    protected override Task OnInitializingAsync()
    {
        Utility.SnapShotId = SelectedSnapshot?.Id ?? throw new ArgumentNullException("未选择快照");
        return Task.CompletedTask;
    }

     async partial void OnSelectedTaskChanged(BackupTask value)
    {
        IsSnapshotComboBoxEnable = false;
        var utility = CreateUtilityImplement();
        Snapshots = new ObservableCollection<BackupSnapshotEntity>(await utility.GetSnapshotsAsync(default));
        if (Snapshots.Count > 0)
        {
            SelectedSnapshot = Snapshots[^1];
        }

        IsSnapshotComboBoxEnable = true;
    }
}