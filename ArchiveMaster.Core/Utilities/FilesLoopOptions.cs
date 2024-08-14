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

        public FilesLoopOptions Build()
        {
            return new FilesLoopOptions()
            {
                AutoApplyProgress = autoApplyProgress,
                AutoApplyStatus = autoApplyStatus,
                CatchAction = catchAction,
                FinallyAction = finallyAction,
                ThrowExceptions = throwExceptions,
                Threads = threads
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
}