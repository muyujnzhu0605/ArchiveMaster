namespace ArchiveMaster.ViewModels.FileSystem;

public class TreeFileInfo : FileSystem.TreeFileDirInfo
{
    public TreeFileInfo()
        : base()
    {
    }
    
    public TreeFileInfo(FileSystemInfo file, string topDir, TreeDirInfo parent, int depth, int index)
        : base(file, topDir, parent, depth, index)
    {
    }

    public TreeFileInfo(FileSystem.SimpleFileInfo file, TreeDirInfo parent, int depth, int index)
        : base(file, parent, depth, index)
    {
    }
}