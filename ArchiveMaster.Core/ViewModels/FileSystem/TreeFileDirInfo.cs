using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public partial class TreeFileDirInfo : SimpleFileInfo
{
    public TreeFileDirInfo()
    {
    }

    public TreeFileDirInfo(FileSystemInfo file, string topDir, TreeDirInfo parent, int depth, int index)
        : base(file, topDir)
    {
        Depth = depth;
        Index = index;
        Parent = parent;
    }

    [ObservableProperty]
    private int index;

    [ObservableProperty]
    private int depth;

    [ObservableProperty]
    private TreeDirInfo parent;

    public bool IsLast()
    {
        return Index == Parent.Subs.Count - 1;
    }
}