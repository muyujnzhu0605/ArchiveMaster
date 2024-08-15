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

        protected async Task<FilesLoopStates> TryForFilesAsync<T>(IEnumerable<T> files, Action<T, FilesLoopStates> body,
            CancellationToken cancellationToken, FilesLoopOptions options = null)
            where T : SimpleFileInfo
        {
            FilesLoopStates states = null;
            await Task.Run(() => { states=TryForFiles(files, body, cancellationToken, options); }, cancellationToken);
            return states;
        }

        protected FilesLoopStates TryForFiles<T>(IEnumerable<T> files, Action<T, FilesLoopStates> body,
            CancellationToken cancellationToken,
            FilesLoopOptions options = null)
            where T : SimpleFileInfo
        {
            options ??= FilesLoopOptions.DoNothing();
            var states = new FilesLoopStates(options);
            
            if (options.AutoApplyProgress == AutoApplyProgressMode.FileLength && options.TotalLength==0)
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

            if (options.TotalCount == 0)
            {
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
            }

            switch (options.Threads)
            {
                case 1:
                    foreach (var file in files)
                    {
                        TryForFilesSingle(body, cancellationToken, options, file, states);
                        if (states.NeedBroken)
                        {
                            break;
                        }
                    }

                    break;
                case >= 2 or 0:
                    Parallel.ForEach(files, new ParallelOptions()
                    {
                        MaxDegreeOfParallelism =options.Threads<=0?-1: options.Threads,
                        CancellationToken = cancellationToken
                    }, (file, s) =>
                    {
                        TryForFilesSingle(body, cancellationToken, options, file, states);
                        if (states.NeedBroken)
                        {
                            s.Break();
                        }
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Threads应为大于等于0的整数，其中1表示单线程，0表示自动多线程，>=2表示指定线程数");
            }

            return states;
        }

        private void TryForFilesSingle<T>(Action<T, FilesLoopStates> body, CancellationToken cancellationToken,
            FilesLoopOptions options, T file,
            FilesLoopStates states) where T : SimpleFileInfo
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
                options.CatchAction?.Invoke(file,ex);
                if (options.ThrowExceptions)
                {
                    throw;
                }
            }
            finally
            {
                if (states.CanAccessTotalLength)
                {
                    states.IncreaseFileLength((file as SimpleFileInfo).Length);
                }

                states.IncreaseFileIndex();

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


            if (AppConfig.Instance.DebugMode && AppConfig.Instance.DebugModeLoopDelay > 0)
            {
                Thread.Sleep(AppConfig.Instance.DebugModeLoopDelay);
            }
        }
    }
}