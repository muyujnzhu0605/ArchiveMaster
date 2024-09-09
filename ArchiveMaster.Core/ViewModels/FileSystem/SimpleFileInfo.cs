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
        [NotifyPropertyChangedFor(nameof(RelativePath))]
        private string path;
        
        [ObservableProperty]
        private DateTime time;
        
        [ObservableProperty]
        private bool isDir;
        
        [ObservableProperty]
        private long length;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RelativePath))]
        private string topDirectory;

        [property: JsonIgnore]
        [ObservableProperty]
        private FileSystemInfo fileSystemInfo;
        

        [JsonIgnore]
        public string RelativePath
        {
            get
            {
                if (string.IsNullOrEmpty(TopDirectory))
                {
                    return Path;
                }
                return System.IO.Path.GetRelativePath(TopDirectory, Path);
            }
        }

        private ProcessStatus status = ProcessStatus.Ready;

        public SimpleFileInfo()
        {
        }

        public SimpleFileInfo(FileSystemInfo file,string topDir)
        {
            ArgumentNullException.ThrowIfNull(file);
            FileSystemInfo = file;
            Name = file.Name;
            Path = file.FullName;
            if (System.IO.Path.IsPathRooted(topDir))
            {
                TopDirectory = topDir;
            }
            else
            {
                throw new ArgumentException($"提供的{nameof(topDir)}不是{nameof(file)}的父级");
            }
            Time = file.LastWriteTime;
            IsDir = file.Attributes.HasFlag(FileAttributes.Directory);
            if (!IsDir && file is FileInfo f)
            {
                Length = f.Length;
            }
        }

        [JsonIgnore]
        public string Message => message;

        [JsonIgnore]
        public bool IsCompleted => status == ProcessStatus.Completed;

        [JsonIgnore]
        public ProcessStatus Status => status;

        [JsonIgnore]
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