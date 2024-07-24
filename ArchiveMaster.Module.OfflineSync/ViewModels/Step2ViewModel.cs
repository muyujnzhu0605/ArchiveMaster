using ArchiveMaster.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using Newtonsoft.Json;
using ArchiveMaster.Model;
using ArchiveMaster.Views;
using System.Collections;
using System.Collections.ObjectModel;
using ArchiveMaster.Enums;
using ArchiveMaster.Configs;
using Mapster;

namespace ArchiveMaster.ViewModels
{
    //public partial class Step2ViewModel : OfflineSyncViewModelBase<SyncFileInfo>
    //{
    //    public Step2Config Config { get; } = AppConfig.Instance.Get<OfflineSyncConfig>().CurrentConfig.Step2;

    //    public Step2ViewModel()
    //    {
    //        Config.Adapt(this);
    //        AppConfig.Instance.BeforeSaving += (s, e) =>
    //        {
    //            this.Adapt(Config);
    //        };
    //    }
    //    [ObservableProperty]
    //    private string localDir;

    //    [ObservableProperty]
    //    private ObservableCollection<LocalAndOffsiteDir> matchingDirs;

    //    [ObservableProperty]
    //    private string offsiteSnapshot;

    //    partial void OnOffsiteSnapshotChanged(string value)
    //    {
    //        MatchingDirs = null;
    //    }

    //    [ObservableProperty]
    //    private string patchDir;

    //    public IEnumerable ExportModes => Enum.GetValues<ExportMode>();
    //}
}