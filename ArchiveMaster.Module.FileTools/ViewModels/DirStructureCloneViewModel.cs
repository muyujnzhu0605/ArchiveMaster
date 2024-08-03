using System.Collections.ObjectModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public partial class DirStructureCloneViewModel : TwoStepViewModelBase
{
    public DirStructureCloneViewModel()
    {
        TargetDir = Config.TargetDir;
        SourceDir = Config.SourceDir;
    }
    [ObservableProperty]
    private string targetDir;

    [ObservableProperty]
    private string sourceDir;

    [ObservableProperty]
    private ObservableCollection<FileInfoWithStatus> files;

    private DirStructureCloneUtility u;
    public DirStructureCloneConfig Config { get; } = AppConfig.Instance.Get<DirStructureCloneConfig>();

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        await u.ExecuteAsync(token);
        u.ProgressUpdate -= Utility_ProgressUpdate;
    }

    protected override async Task InitializeImplAsync()
    {
        if (string.IsNullOrEmpty(SourceDir))
        {
            throw new Exception("源目录为空");
        }

        if (!Directory.Exists(SourceDir))
        {
            throw new Exception("源目录不存在");
        }

        if (string.IsNullOrEmpty(TargetDir))
        {
            throw new Exception("目标目录为空");
        }


        Config.SourceDir = SourceDir;
        Config.TargetDir = TargetDir;
        u = new DirStructureCloneUtility(Config);
        u.ProgressUpdate += Utility_ProgressUpdate;
        await u.InitializeAsync();
        Files = new ObservableCollection<FileInfoWithStatus>(u.Files);
    }


    protected override void ResetImpl()
    {
        Files = null;
    }
}