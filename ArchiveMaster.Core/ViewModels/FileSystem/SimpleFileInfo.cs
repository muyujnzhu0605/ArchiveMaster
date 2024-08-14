using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Text.Json.Serialization;
using ArchiveMaster.Enums;

namespace ArchiveMaster.ViewModels
{
    public partial class SimpleFileInfo : ObservableObject
    {
        [property: JsonIgnore]
        [ObservableProperty]
        private bool isChecked = true;

        private string message;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string path;
        
        [ObservableProperty]
        private DateTime time;
        
        [ObservableProperty]
        private bool isDir;
        
        [ObservableProperty]
        private long length;

        private ProcessStatus status = ProcessStatus.Ready;

        public SimpleFileInfo()
        {
        }

        public SimpleFileInfo(FileSystemInfo file)
        {
            ArgumentNullException.ThrowIfNull(file);
            Name = file.Name;
            Path = file.FullName;
            Time = file.LastWriteTime;
            IsDir = file.Attributes.HasFlag(FileAttributes.Directory);
            if (!IsDir && file is FileInfo f)
            {
                Length = f.Length;
            }
        }

        public string Message => message;

        public bool IsCompleted => status == ProcessStatus.Completed;

        [JsonIgnore]
        public ProcessStatus Status => status;

        public bool Exists => File.Exists(Path);

        public void Complete()
        {
            status = ProcessStatus.Completed;
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
        }

        public void Error(Exception ex)
        {
            status = ProcessStatus.Error;
            message = ex.Message;
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(Message));
        }

        public void Warn(string msg)
        {
            status = ProcessStatus.Warn;
            message = msg;
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(Message));
        }

        public void Processing()
        {
            status = ProcessStatus.Processing;
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
        }
    }
}