using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FzLib;

namespace ArchiveMaster.ViewModels;

public partial class DirStructureSyncViewModel(DirStructureSyncConfig config)
    : TwoStepViewModelBase<DirStructureSyncUtility, DirStructureSyncConfig>(config)
{
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

    partial void OnDisplayMultipleMatchesChanged(bool value)
    {
        UpdateList();
    }

    partial void OnDisplayRightPositionChanged(bool value)
    {
        UpdateList();
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
        if (Utility == null)
        {
            return;
        }

        if (Utility.WrongPositionFiles == null || Utility.RightPositionFiles == null)
        {
            Files = new ObservableCollection<MatchingFileInfo>();
            return;
        }

        IEnumerable<MatchingFileInfo> files = Utility.WrongPositionFiles;
        if (DisplayRightPosition)
        {
            files = files.Concat(Utility.RightPositionFiles);
        }

        if (!DisplayMultipleMatches)
        {
            files = files.Where(p => p.MultipleMatches == false);
        }

        files = files.OrderBy(p => p.Path);
        Files = new ObservableCollection<MatchingFileInfo>(files);
        Utility.ExecutingFiles = Files;
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