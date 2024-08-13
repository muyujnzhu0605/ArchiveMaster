using System.Collections;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Utilities
{
    public abstract class UtilityBase
    {
        public event EventHandler<ProgressUpdateEventArgs> ProgressUpdate;
        public event EventHandler<MessageUpdateEventArgs> MessageUpdate;
        public abstract ConfigBase Config { get; }

        protected void NotifyProgressIndeterminate()
        {
            NotifyProgress(double.NaN);
        }

        protected void NotifyProgress(double percent)
        {
            ProgressUpdate?.Invoke(this, new ProgressUpdateEventArgs(percent));
        }

        protected void NotifyMessage(string message)
        {
            MessageUpdate?.Invoke(this, new MessageUpdateEventArgs(message));
        }

        protected Task TryForFilesAsync<T>(IEnumerable<T> files, Action<T, FilesLoopStates> body,
            CancellationToken cancellationToken, FilesLoopOptions options = null)
            where T : SimpleFileOrDirInfo
        {
            return Task.Run(() => { TryForFiles(files, body, cancellationToken, options); }, cancellationToken);
        }

        protected void TryForFiles<T>(IEnumerable<T> files, Action<T, FilesLoopStates> body,
            CancellationToken cancellationToken,
            FilesLoopOptions options = null)
            where T : SimpleFileOrDirInfo
        {
            options ??= new FilesLoopOptions();
            var states = new FilesLoopStates(options);
            switch (options.AutoApplyProgress)
            {
                case AutoApplyProgressMode.FileLength:
                    foreach (var file in files)
                    {
                        if (file is not SimpleFileInfo f)
                        {
                            throw new ArgumentException(
                                "集合内的元素必须都为SimpleFileInfo时，才可以使用AutoApplyProgressMode.FileLength",
                                nameof(files));
                        }

                        states.TotalLength += f.Length;
                    }

                    states.CanAccessTotalLength = true;

                    break;
                case AutoApplyProgressMode.FileNumber:
                    states.FileCount = files.Count();
                    break;
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    body(file, states);
                    if (options.AutoApplyStatus)
                    {
                        if (file.Status == ProcessStatus.Ready)
                        {
                            file.Complete();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    file.Error(ex);
                    options.CatchAction?.Invoke(file);
                }
                finally
                {
                    if (states.CanAccessTotalLength)
                    {
                        states.AccumulatedLength += (file as SimpleFileInfo).Length;
                    }

                    states.FileIndex++;

                    switch (options.AutoApplyProgress)
                    {
                        case AutoApplyProgressMode.FileLength:
                            NotifyProgress(1.0 * states.AccumulatedLength / states.TotalLength);
                            break;
                        case AutoApplyProgressMode.FileNumber:
                            NotifyProgress(1.0 * states.FileIndex / states.FileCount);
                            break;
                    }
                    options.FinnalyAction?.Invoke(file);
                }

                if (states.NeedBroken)
                {
                    break;
                }

                if (AppConfig.Instance.DebugMode && AppConfig.Instance.DebugModeLoopDelay > 0)
                {
                    Thread.Sleep(AppConfig.Instance.DebugModeLoopDelay);
                }
            }
        }

        public class FilesLoopStates(FilesLoopOptions options)
        {
            private long totalLength = 0;

            public FilesLoopOptions Options { get; } = options;
            internal bool NeedBroken { get; private set; }

            public int FileCount { get; internal set; }

            public int FileIndex { get; internal set; }

            public long TotalLength
            {
                get => CanAccessTotalLength ? totalLength : throw new ArgumentException("未初始化总大小，不可调用TotalLength");
                internal set => totalLength = value;
            }

            internal bool CanAccessTotalLength { get; set; }

            public long AccumulatedLength { get; internal set; }

            public static string ProgressMessageFormat { get; set; } = "（{0}/{1}）";

            public string GetFileIndexAndCountMessage()
            {
                int fileIndex = FileCount + 1;
                return string.Format(ProgressMessageFormat, fileIndex, FileCount);
            }

            public void Break()
            {
                NeedBroken = true;
            }
        }

        public class FilesLoopOptions
        {
            public FilesLoopOptions()
            {
            }

            public FilesLoopOptions(bool autoApplyStatus)
            {
                AutoApplyStatus = autoApplyStatus;
            }

            public FilesLoopOptions(bool autoApplyStatus, AutoApplyProgressMode autoApplyProgress)
            {
                AutoApplyStatus = autoApplyStatus;
                AutoApplyProgress = autoApplyProgress;
            }

            public FilesLoopOptions(AutoApplyProgressMode autoApplyProgress)
            {
                AutoApplyProgress = autoApplyProgress;
            }

            public bool AutoApplyStatus { get; set; } = true;
            public AutoApplyProgressMode AutoApplyProgress { get; set; } = AutoApplyProgressMode.FileNumber;
            
            public Action<SimpleFileOrDirInfo> CatchAction { get; set; }
            
            public Action<SimpleFileOrDirInfo> FinnalyAction { get; set; }
        }

        public enum AutoApplyProgressMode
        {
            None,
            FileNumber,
            FileLength
        }
    }
}