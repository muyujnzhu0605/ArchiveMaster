using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchiveMaster.Configs;
using ArchiveMaster.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib;
using OffsiteBackupOfflineSync.Utility;

namespace ArchiveMaster.ViewModels;

public partial class DirStructureSyncViewModel : TwoStepViewModelBase<DirStructureSyncUtility>
{
    public override DirStructureSyncConfig Config { get; } = AppConfig.Instance.Get<DirStructureSyncConfig>();

    [ObservableProperty]
    private bool displayMultipleMatches = true;

    [ObservableProperty]
    private bool displayRightPosition = false;

    [ObservableProperty]
    private ObservableCollection<MatchingFileInfo> files;

    [ObservableProperty]
    private int filesCount = 0;

    [ObservableProperty]
    private int checkedFilesCount = 0;

    private DirStructureSyncUtility u;

    partial void OnDisplayMultipleMatchesChanged(bool value)
    {
        UpdateList();
    }

    partial void OnDisplayRightPositionChanged(bool value)
    {
        UpdateList();
    }

    protected override Task OnInitializingAsync()
    {
        if (string.IsNullOrEmpty(Config.TemplateDir))
        {
            throw new Exception("模板目录为空");
        }

        if (!Directory.Exists(Config.TemplateDir))
        {
            throw new Exception("模板目录不存在");
        }

        if (string.IsNullOrEmpty(Config.SourceDir))
        {
            throw new Exception("源目录为空");
        }

        if (!Directory.Exists(Config.SourceDir))
        {
            throw new Exception("源目录不存在");
        }

        return base.OnInitializingAsync();
    }


    protected override Task OnInitializedAsync()
    {
        UpdateList();
        return base.OnInitializedAsync();
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


    protected override void OnReset()
    {
        Files = null;
        FilesCount = 0;
        CheckedFilesCount = 0;
    }
}