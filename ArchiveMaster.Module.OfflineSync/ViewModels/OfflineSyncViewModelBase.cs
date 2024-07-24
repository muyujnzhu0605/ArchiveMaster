using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Messages;
using ArchiveMaster.UI.ViewModels;
using ArchiveMaster.Utility;
using ArchiveMaster.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib;
using FzLib.Avalonia.Messages;
using Mapster;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ArchiveMaster.ViewModels
{
    public abstract partial class OfflineSyncViewModelBase<T> : ObservableObject where T : FileInfoWithStatus
    {
        [ObservableProperty]
        private bool canAnalyze = true;

        [ObservableProperty]
        private bool canEditConfigs = true;

        [ObservableProperty]
        private bool canProcess = false;

        [ObservableProperty]
        private bool canStop = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Config))]
        private string configName = AppConfig.Instance.Get<OfflineSyncConfig>().CurrentConfigName;

        [ObservableProperty]
        private ObservableCollection<string> configNames = new ObservableCollection<string>(
            AppConfig.Instance.Get<OfflineSyncConfig>().ConfigCollection.Keys);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AddedFileLength),
                nameof(AddedFileCount),
                nameof(ModifiedFileCount),
                nameof(ModifiedFileLength),
                nameof(DeletedFileCount),
                nameof(MovedFileCount),
                nameof(CheckedFileCount))]
        private ObservableCollection<T> files = new ObservableCollection<T>();

        [ObservableProperty]
        private string message = "就绪";

        [ObservableProperty]
        private double progress;

        [ObservableProperty]
        private bool progressIndeterminate;

        [ObservableProperty]
        private double progressMax = 1;

        public OfflineSyncViewModelBase()
        {
            Config.Adapt(this, Config.GetType(), GetType());
            AppConfig.Instance.BeforeSaving += (s, e) =>
            {
                this.Adapt(Config, GetType(), Config.GetType());
            };
            Utility.MessageReceived += (s, e) =>
            {
                Message = e.Message;
            };
            Utility.ProgressUpdated += (s, e) =>
            {
                if (e.MaxValue != ProgressMax)
                {
                    ProgressMax = e.MaxValue;
                }
                Progress = e.Value;
            };
        }

        public long AddedFileCount => Files?.Cast<SyncFileInfo>().Where(p => p.UpdateType == FileUpdateType.Add && p.IsChecked)?.Count() ?? 0;

        public long AddedFileLength => Files?.Cast<SyncFileInfo>().Where(p => p.UpdateType == FileUpdateType.Add && p.IsChecked)?.Sum(p => p.Length) ?? 0;

        public int CheckedFileCount => Files?.Where(p => p.IsChecked)?.Count() ?? 0;

        public int DeletedFileCount => Files?.Cast<SyncFileInfo>().Where(p => p.UpdateType == FileUpdateType.Delete && p.IsChecked)?.Count() ?? 0;

        public long ModifiedFileCount => Files?.Cast<SyncFileInfo>().Where(p => p.UpdateType == FileUpdateType.Modify && p.IsChecked)?.Count() ?? 0;

        public long ModifiedFileLength => Files?.Cast<SyncFileInfo>().Where(p => p.UpdateType == FileUpdateType.Modify && p.IsChecked)?.Sum(p => p.Length) ?? 0;

        public int MovedFileCount => Files?.Cast<SyncFileInfo>().Where(p => p.UpdateType == FileUpdateType.Move && p.IsChecked)?.Count() ?? 0;

        protected abstract OfflineSyncStepConfigBase Config { get; }
        protected abstract OfflineSyncUtilityBase Utility { get; }
        protected Task ShowErrorAsync(string title, Exception exception)
        {
            return WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Error,
                Title = title,
                Exception = exception
            }).Task;
        }

        protected void UpdateStatus(StatusType status)
        {
            CanStop = status is StatusType.Analyzing or StatusType.Processing;
            CanAnalyze = status is StatusType.Ready or StatusType.Analyzed;
            CanProcess = status is StatusType.Analyzed;
            CanEditConfigs = status is StatusType.Ready or StatusType.Analyzed;
            Message = status is StatusType.Ready or StatusType.Analyzed ? "就绪" : "处理中";
            Progress = 0;
            ProgressIndeterminate = status is StatusType.Analyzing or StatusType.Processing or StatusType.Stopping;
        }

        [RelayCommand]
        private async Task AddConfigAsync()
        {
            if (await this.SendMessage(new InputDialogMessage()
            {
                Type = InputDialogMessage.InputDialogType.Text,
                Title = "新增配置",
                DefaultValue = "新配置",
                Validation = t =>
                {
                    if (string.IsNullOrWhiteSpace(t))
                    {
                        throw new Exception("配置名为空");
                    }
                    if (ConfigNames.Contains(t))
                    {
                        throw new Exception("配置名已存在");
                    }
                }
            }).Task is string result)
            {
                ConfigNames.Add(result);
                AppConfig.Instance.Get<OfflineSyncConfig>().ConfigCollection.Add(result, new SingleConfig());
                ConfigName = result;
            }
        }

        private void AddFileCheckedNotify(FileInfoWithStatus file)
        {
            file.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FileInfoWithStatus.IsChecked))
                {
                    this.Notify(nameof(CheckedFileCount));
                    if (s is SyncFileInfo syncFile)
                    {
                        switch (syncFile.UpdateType)
                        {
                            case FileUpdateType.Add:
                                this.Notify(nameof(AddedFileCount), nameof(AddedFileLength));
                                break;

                            case FileUpdateType.Modify:
                                this.Notify(nameof(ModifiedFileCount), nameof(ModifiedFileLength));
                                break;

                            case FileUpdateType.Delete:
                                this.Notify(nameof(DeletedFileCount));
                                break;

                            case FileUpdateType.Move:
                                this.Notify(nameof(MovedFileCount));
                                break;

                            default:
                                break;
                        }
                    }
                }
            };
        }

        partial void OnConfigNameChanged(string oldValue, string newValue)
        {
            if (!string.IsNullOrEmpty(oldValue))
            {
                this.Adapt(Config, GetType(), Config.GetType());
            }
            if (AppConfig.Instance.Get<OfflineSyncConfig>().CurrentConfigName != newValue)
            {
                AppConfig.Instance.Get<OfflineSyncConfig>().CurrentConfigName = newValue;
            }
            if (!string.IsNullOrEmpty(newValue))
            {
                Config.Adapt(this, Config.GetType(), GetType());
            }
        }

        partial void OnFilesChanged(ObservableCollection<T> value)
        {
            value.ForEach(p => AddFileCheckedNotify(p));
            value.CollectionChanged += (s, e) => throw new NotSupportedException("不允许对集合进行修改");
        }

        partial void OnProgressChanged(double value)
        {
            ProgressIndeterminate = false;
        }

        [RelayCommand]
        private void RemoveConfig()
        {
            var name = ConfigName;
            ConfigNames.Remove(name);
            AppConfig.Instance.Get<OfflineSyncConfig>().ConfigCollection.Remove(name);
            if (ConfigNames.Count == 0)
            {
                ConfigNames.Add("默认");
                ConfigName = "默认";
            }
            else
            {
                ConfigName = ConfigNames[0];
            }
        }
    }
}