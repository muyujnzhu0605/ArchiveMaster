using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class RenameConfig : ConfigBase
{
    [ObservableProperty]
    private string dir;

    [ObservableProperty]
    private string searchPattern;

    [ObservableProperty]
    private string replacePattern;

    [ObservableProperty]
    private RenameTargetType renameTarget = RenameTargetType.File;

    [ObservableProperty]
    private SearchMode searchMode = SearchMode.Contain;

    [ObservableProperty]
    private RenameMode renameMode = RenameMode.ReplaceMatched;

    [ObservableProperty]
    private bool searchPath;

    [ObservableProperty]
    private bool includeSubDirs;

    public override void Check()
    {
    }
}