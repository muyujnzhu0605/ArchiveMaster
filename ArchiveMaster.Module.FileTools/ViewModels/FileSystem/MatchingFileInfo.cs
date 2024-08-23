using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public partial class MatchingFileInfo : SimpleFileInfo
{
    public MatchingFileInfo(FileInfo file, string topDir) : base(file, topDir)
    {
    }

    [ObservableProperty]
    private bool multipleMatches;

    [ObservableProperty]
    private bool rightPosition;

    [ObservableProperty]
    private SimpleFileInfo template;
}