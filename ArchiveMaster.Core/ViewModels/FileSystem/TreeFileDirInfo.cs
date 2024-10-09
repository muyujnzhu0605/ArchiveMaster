using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace ArchiveMaster.ViewModels;

public partial class TreeFileDirInfo : SimpleFileInfo
{
    [ObservableProperty]
    private int depth;

    [property: JsonIgnore]
    [ObservableProperty]
    private int index;

    [property: JsonIgnore]
    [ObservableProperty]
    private TreeDirInfo parent;

    internal TreeFileDirInfo()
    {
    }

    internal TreeFileDirInfo(FileSystemInfo file, string topDir, TreeDirInfo parent, int depth, int index)
        : base(file, topDir)
    {
        Depth = depth;
        Index = index;
        Parent = parent;
    }

    internal TreeFileDirInfo(SimpleFileInfo file, TreeDirInfo parent, int depth, int index)
        : base(file)
    {
        Depth = depth;
        Index = index;
        Parent = parent;
    }
    public bool IsLast()
    {
        return Index == Parent.Subs.Count - 1;
    }
}