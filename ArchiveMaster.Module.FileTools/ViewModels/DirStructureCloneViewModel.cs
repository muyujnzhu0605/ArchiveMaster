using System.Collections.ObjectModel;
using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Services;
using ArchiveMaster.ViewModels.FileSystem;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public partial class
    DirStructureCloneViewModel(DirStructureCloneConfig config, AppConfig appConfig)
    : TwoStepViewModelBase<DirStructureCloneService, DirStructureCloneConfig>(config, appConfig)
{
    [ObservableProperty]
    private BulkObservableCollection<SimpleFileInfo> treeFiles;

    protected override Task OnInitializedAsync()
    {
        var files = new BulkObservableCollection<SimpleFileInfo>();
        files.AddRange(Service.RootDir.Subs);
        TreeFiles = files;
        return base.OnInitializedAsync();
    }


    protected override void OnReset()
    {
        TreeFiles = null;
    }
}