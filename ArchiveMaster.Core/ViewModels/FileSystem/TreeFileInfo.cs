namespace ArchiveMaster.ViewModels;

public class TreeFileInfo : TreeFileDirInfo
{
    public TreeFileInfo(FileSystemInfo file, string topDir, TreeDirInfo parent, int depth, int index)
        : base(file, topDir, parent, depth, index)
    {
    }
}