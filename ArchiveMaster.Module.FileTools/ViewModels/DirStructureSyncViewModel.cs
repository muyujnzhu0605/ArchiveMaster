using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchiveMaster.Configs;
using ArchiveMaster.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib;
using OffsiteBackupOfflineSync.Utility;

namespace ArchiveMaster.ViewModels;

public partial class DirStructureSyncViewModel : TwoStepViewModelBase
{
    public DirStructureSyncViewModel()
    {
        SourceDir = Config.SourceDir;
        TemplateDir = Config.TemplateDir;
        TargetDir = Config.TargetDir;
    }
    public DirStructureSyncConfig Config { get; } = AppConfig.Instance.Get<DirStructureSyncConfig>();

    [ObservableProperty]
    private bool displayMultipleMatches = true;

    [ObservableProperty]
    private bool displayRightPosition = false;

    [ObservableProperty]
    private string sourceDir;

    [ObservableProperty]
    private string templateDir;

    [ObservableProperty]
    private string targetDir;

    [ObservableProperty]
    private ObservableCollection<MatchingFileInfo> files;

    [ObservableProperty]
    private int filesCount = 0;

    [ObservableProperty]
    private int checkedFilesCount = 0;

    private DirStructureSyncUtility u;

    partial void OnSourceDirChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(TargetDir) || oldValue == TargetDir)
        {
            TargetDir = newValue;
        }
    }

    partial void OnDisplayMultipleMatchesChanged(bool value)
    {
        UpdateList();
    }

    partial void OnDisplayRightPositionChanged(bool value)
    {
        UpdateList();
    }


    protected override async Task InitializeImplAsync()
    {
        if (string.IsNullOrEmpty(TemplateDir))
        {
            throw new Exception("模板目录为空");
        }

        if (!Directory.Exists(TemplateDir))
        {
            throw new Exception("模板目录不存在");
        }

        if (string.IsNullOrEmpty(SourceDir))
        {
            throw new Exception("源目录为空");
        }

        if (!Directory.Exists(SourceDir))
        {
            throw new Exception("源目录不存在");
        }

        Config.TemplateDir = TemplateDir;
        Config.SourceDir = SourceDir;
        Config.TargetDir = TargetDir;


        u = new DirStructureSyncUtility(Config);
        await u.InitializeAsync();
        UpdateList();
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        await u.ExecuteAsync(token);
    }

    [RelayCommand]
    private void SelectAll()
    {
        Files?.ForEach(p => p.IsChecked = true);
    }

    [RelayCommand]
    private void SelectNone()
    {
        Files?.ForEach(p => p.IsChecked = true);
    }

    private void UpdateList()
    {
        if (u == null)
        {
            return;
        }

        if (u.WrongPositionFiles == null || u.RightPositionFiles == null)
        {
            Files = new ObservableCollection<MatchingFileInfo>();
            return;
        }

        IEnumerable<MatchingFileInfo> files = u.WrongPositionFiles;
        if (DisplayRightPosition)
        {
            files = files.Concat(u.RightPositionFiles);
        }

        if (!DisplayMultipleMatches)
        {
            files = files.Where(p => p.MultipleMatches == false);
        }

        files = files.OrderBy(p => p.Path);
        Files = new ObservableCollection<MatchingFileInfo>(files);
        u.ExecutingFiles = Files;
        FilesCount = Files.Count;
        CheckedFilesCount = Files.Count(p => p.IsChecked);
    }


    protected override void ResetImpl()
    {
        Files = null;
        FilesCount = 0;
        CheckedFilesCount = 0;
    }
}