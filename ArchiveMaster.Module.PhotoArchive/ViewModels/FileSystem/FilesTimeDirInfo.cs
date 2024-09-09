using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public partial class FilesTimeDirInfo : SimpleFileInfo
{
    public FilesTimeDirInfo()
    {
        IsDir = true;
    }

    public FilesTimeDirInfo(DirectoryInfo dir, string topDir) : base(dir, topDir)
    {
        var subFiles = dir.EnumerateFiles().ToList();
        Subs = new List<SimpleFileInfo>(subFiles.Select(p => new SimpleFileInfo(p, topDir)).ToList());
        FilesCount = subFiles.Count;
        if (FilesCount > 0)
        {
            EarliestTime = new DateTime(subFiles
                .Select(p => p.LastWriteTime)
                .Select(p => p.Ticks)
                .Min());
            LatestTime = new DateTime(subFiles
                .Select(p => p.LastWriteTime)
                .Select(p => p.Ticks)
                .Max());
        }
    }

    [ObservableProperty]
    private int filesCount;

    [ObservableProperty]
    private DateTime earliestTime;

    [ObservableProperty]
    private DateTime latestTime;

    public IList<SimpleFileInfo> Subs { get; } = new List<SimpleFileInfo>();
}