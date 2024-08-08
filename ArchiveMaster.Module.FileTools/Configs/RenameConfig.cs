using ArchiveMaster.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.Configs;

public partial class RenameConfig : ConfigBase
{

    [ObservableProperty]
    private string dir;
    
    [ObservableProperty]
    private string search;
    
    [ObservableProperty]
    private string replace;

    [ObservableProperty]
    private RenameTargetType renameTarget;
    
    [ObservableProperty]
    private SearchMode searchMode;
    
    [ObservableProperty]
    private RenameMode renameMode;
    
    [ObservableProperty]
    private bool includeSubjectDirectories;

    public override void Check()
    {
    }
}
