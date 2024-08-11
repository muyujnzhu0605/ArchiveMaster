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

                        states.MaxProgress += f.Length;
                    }

                    break;
                case AutoApplyProgressMode.FileNumber:
                    states.MaxProgress = files.Count();
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
                }
                finally
                {
                    switch (options.AutoApplyProgress)
                    {
                        case AutoApplyProgressMode.FileLength:
                            states.Progress += (file as SimpleFileInfo).Length;
                            break;
                        case AutoApplyProgressMode.FileNumber:
                            states.Progress++;
                            break;
                    }

                    if (options.AutoApplyProgress is not AutoApplyProgressMode.None)
                    {
                        NotifyProgress(1.0 * states.Progress / states.MaxProgress);
                    }
                }

                if (states.NeedBroken)
                {
                    break;
                }
            }
        }

        public class FilesLoopStates(FilesLoopOptions options)
        {
            public FilesLoopOptions Options { get; } = options;
            internal bool NeedBroken { get; private set; }
            internal long MaxProgress { get; set; }
            internal long Progress { get; set; }

            public static string ProgressMessageFormat { get; set; } = "（{Progress}/{MaxProgress}）";

            public string GetProgressMessage()
            {
                long progress = Options.AutoApplyProgress switch
                {
                    AutoApplyProgressMode.FileNumber => Progress + 1,
                    AutoApplyProgressMode.FileLength => Progress,
                    AutoApplyProgressMode.None or _ => throw new ArgumentException(
                        "当AutoApplyProgressMode为None时，不支持调用该方法"),
                };
                return ProgressMessageFormat
                    .Replace("{Progress}", progress.ToString())
                    .Replace("{MaxProgress}", MaxProgress.ToString());
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

            public bool AutoApplyStatus { get; set; } = true;
            public AutoApplyProgressMode AutoApplyProgress { get; set; } = AutoApplyProgressMode.FileNumber;
        }

        public enum AutoApplyProgressMode
        {
            None,
            FileNumber,
            FileLength
        }
    }
}