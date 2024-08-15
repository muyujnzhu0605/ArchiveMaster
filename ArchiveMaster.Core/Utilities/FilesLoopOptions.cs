using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Utilities;

public class FilesLoopOptions
{
    public static FilesLoopOptions DoNothing()
    {
        return new FilesLoopOptions();
    }

    public static FilesLoopOptionsBuilder Builder()
    {
        return new FilesLoopOptionsBuilder();
    }

    public class FilesLoopOptionsBuilder
    {
        private AutoApplyProgressMode autoApplyProgress;

        private bool autoApplyStatus;

        private Action<SimpleFileInfo, Exception> catchAction;

        private Action<SimpleFileInfo> finallyAction;

        private bool throwExceptions;

        private int threads = 1;

        private int totalCount = 0;
        private long totalLength = 0;
        private int initialCount = 0;
        private long initialLength = 0;

        public FilesLoopOptionsBuilder AutoApplyFileNumberProgress()
        {
            autoApplyProgress = AutoApplyProgressMode.FileNumber;
            return this;
        }

        public FilesLoopOptionsBuilder AutoApplyFileLengthProgress()
        {
            autoApplyProgress = AutoApplyProgressMode.FileLength;
            return this;
        }

        public FilesLoopOptionsBuilder AutoApplyStatus()
        {
            autoApplyStatus = true;
            return this;
        }

        public FilesLoopOptionsBuilder Catch(Action<SimpleFileInfo, Exception> action)
        {
            catchAction = action;
            return this;
        }

        public FilesLoopOptionsBuilder Finally(Action<SimpleFileInfo> action)
        {
            finallyAction = action;
            return this;
        }

        public FilesLoopOptionsBuilder ThrowExceptions()
        {
            throwExceptions = true;
            return this;
        }

        public FilesLoopOptionsBuilder WithMultiThreads()
        {
            threads = 0;
            return this;
        }

        public FilesLoopOptionsBuilder WithMultiThreads(int threadCount)
        {
            threads = threadCount;
            return this;
        }

        public FilesLoopOptionsBuilder SetCount(int initial, int total)
        {
            initialCount = initial;
            totalCount = total;
            return this;
        }


        public FilesLoopOptionsBuilder SetLength(long initial, long total)
        {
            initialLength = initial;
            totalLength = total;
            return this;
        }


        public FilesLoopOptions Build()
        {
            return new FilesLoopOptions()
            {
                AutoApplyProgress = autoApplyProgress,
                AutoApplyStatus = autoApplyStatus,
                CatchAction = catchAction,
                FinallyAction = finallyAction,
                ThrowExceptions = throwExceptions,
                Threads = threads,
                InitialCount = initialCount,
                InitialLength = initialLength,
                TotalCount = totalCount,
                TotalLength = totalLength,
            };
        }
    }

    private FilesLoopOptions()
    {
    }

    public bool AutoApplyStatus { get; init; }
    public AutoApplyProgressMode AutoApplyProgress { get; init; } = AutoApplyProgressMode.None;

    public Action<SimpleFileInfo, Exception> CatchAction { get; init; }

    public Action<SimpleFileInfo> FinallyAction { get; init; }

    public bool ThrowExceptions { get; init; }

    public int Threads { get; init; } = 1;

    public int TotalCount { get; set; }

    public long TotalLength { get; set; }
    
    public int InitialCount { get; set; }
    
    public long InitialLength { get; set; }
}