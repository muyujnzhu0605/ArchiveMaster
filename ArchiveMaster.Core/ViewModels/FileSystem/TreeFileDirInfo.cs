using System.Text.Json.Serialization;
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

    [property: JsonIgnore] 
    [ObservableProperty]
    private int index;

    [ObservableProperty]
    private int depth;

    [property: JsonIgnore] 
    [ObservableProperty]
    private TreeDirInfo parent;

    public bool IsLast()
    {
        return Index == Parent.Subs.Count - 1;
    }
}