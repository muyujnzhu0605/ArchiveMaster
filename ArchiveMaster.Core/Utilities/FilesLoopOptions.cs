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

        private Action<SimpleFileOrDirInfo> catchAction;

        private Action<SimpleFileOrDirInfo> finallyAction;

        private bool throwExceptions;

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
        
        public FilesLoopOptionsBuilder Catch(Action<SimpleFileOrDirInfo> action)
        {
            catchAction = action;
            return this;
        }
        
        public FilesLoopOptionsBuilder Finally(Action<SimpleFileOrDirInfo> action)
        {
            finallyAction = action;
            return this;
        }

        public FilesLoopOptionsBuilder ThrowExceptions()
        {
            throwExceptions = true;
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
                ThrowExceptions = throwExceptions
            };
        }
    }

    private FilesLoopOptions()
    {
    }

    public bool AutoApplyStatus { get; init; }
    public AutoApplyProgressMode AutoApplyProgress { get; init; } = AutoApplyProgressMode.None;

    public Action<SimpleFileOrDirInfo> CatchAction { get; init; }

    public Action<SimpleFileOrDirInfo> FinallyAction { get; init; }

    public bool ThrowExceptions { get; init; }
}