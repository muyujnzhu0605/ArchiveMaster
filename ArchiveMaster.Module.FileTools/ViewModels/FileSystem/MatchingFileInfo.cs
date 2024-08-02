using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public partial class MatchingFileInfo : FileInfoWithStatus
{
    [ObservableProperty]
    private bool multipleMatches;
    [ObservableProperty]
    private bool rightPosition;
    [ObservableProperty] 
    private SimpleFileInfo template;
}