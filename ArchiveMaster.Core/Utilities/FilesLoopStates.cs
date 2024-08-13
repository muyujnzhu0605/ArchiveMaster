namespace ArchiveMaster.Utilities;

public class FilesLoopStates()
{
    private long totalLength = 0;
    private int fileCount = 0;

    internal bool NeedBroken { get; private set; }

    public int FileCount
    {
        get => CanAccessFileCount ? fileCount : throw new ArgumentException("未初始化文件总数，不可调用TotalLength");
        internal set
        {
            fileCount = value;
            CanAccessFileCount = true;
        }
    }

    public int FileIndex { get; internal set; }

    public long TotalLength
    {
        get => CanAccessTotalLength ? totalLength : throw new ArgumentException("未初始化总大小，不可调用TotalLength");
        internal set
        {
            totalLength = value;
            CanAccessTotalLength = true;
        }
    }

    internal bool CanAccessTotalLength { get; set; }

    internal bool CanAccessFileCount { get; set; }

    public long AccumulatedLength { get; internal set; }

    public static string ProgressMessageFormat { get; set; } = "（{0}/{1}）";

    public static string ProgressMessageIndexOnlyFormat { get; set; } = "（{0}个）";

    public string GetFileNumberMessage()
    {
        int fileIndex = FileIndex + 1;
        if (CanAccessFileCount)
        {
            return string.Format(ProgressMessageFormat, fileIndex, FileCount);
        }

        return string.Format(ProgressMessageIndexOnlyFormat, fileIndex);
    }

    public void Break()
    {
        NeedBroken = true;
    }
}