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
            options ??= FilesLoopOptions.DoNothing();
            var states = new FilesLoopStates();

            if (options.AutoApplyProgress == AutoApplyProgressMode.FileLength)
            {
                states.CanAccessTotalLength = true;

                foreach (var file in files)
                {
                    if (file is not SimpleFileInfo f)
                    {
                        states.CanAccessTotalLength = false;
                        throw new ArgumentException(
                            "集合内的元素必须都为SimpleFileInfo时，才可以使用AutoApplyProgressMode.FileLength",
                            nameof(files));
                    }

                    states.TotalLength += f.Length;
                }
            }

            if (files is ICollection collection)
            {
                states.FileCount = collection.Count;
            }
            else
            {
                if (options.AutoApplyProgress == AutoApplyProgressMode.FileNumber)
                {
                    throw new ArgumentException(
                        "必须为集合，才可以使用AutoApplyProgressMode.FileNumber",
                        nameof(files));
                }
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
                    if (options.ThrowExceptions)
                    {
                        throw;
                    }
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

                    options.FinallyAction?.Invoke(file);
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

      
    }
}