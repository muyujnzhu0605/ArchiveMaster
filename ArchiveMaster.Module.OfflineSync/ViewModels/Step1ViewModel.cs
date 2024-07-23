using ArchiveMaster.Configs;
using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using Mapster;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace ArchiveMaster.ViewModels
{
    public partial class Step1ViewModel : OfflineSyncViewModelBase<FileInfoWithStatus>
    {
        [ObservableProperty]
        private string outputFile;

        [ObservableProperty]
        private ObservableCollection<string> syncDirs = new ObservableCollection<string>();

        public Step1ViewModel()
        {
            Config.Adapt(this);
            AppConfig.Instance.BeforeSaving += (s, e) =>
            {
                this.Adapt(Config);
            };
        }

        public Step1Config Config { get; } = AppConfig.Instance.Get<OfflineSyncConfig>().CurrentConfig.Step1;
        public void AddSyncDir(string path)
        {
            DirectoryInfo newDirInfo = new DirectoryInfo(path);

            // 检查新目录与现有目录是否相同
            foreach (string existingPath in SyncDirs)
            {
                DirectoryInfo existingDirInfo = new DirectoryInfo(existingPath);

                if (existingDirInfo.FullName.Equals(newDirInfo.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"目录 '{path}' 已经存在，不能重复添加。");
                }
            }

            // 检查新目录是否是现有目录的子目录或父目录
            foreach (string existingPath in SyncDirs)
            {
                DirectoryInfo existingDirInfo = new DirectoryInfo(existingPath);

                // 检查新目录是否是现有目录的子目录
                DirectoryInfo temp = newDirInfo;
                while (temp.Parent != null)
                {
                    if (temp.Parent.FullName.Equals(existingDirInfo.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"新目录 '{path}' 是现有目录 '{existingPath}' 的子目录，不能添加。");
                    }
                    temp = temp.Parent;
                }

                // 检查新目录是否是现有目录的父目录
                temp = existingDirInfo;
                while (temp.Parent != null)
                {
                    if (temp.Parent.FullName.Equals(newDirInfo.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"新目录 '{path}' 是现有目录 '{existingPath}' 的父目录，不能添加。");
                    }
                    temp = temp.Parent;
                }
            }

            SyncDirs.Add(path);

        }
    }
}