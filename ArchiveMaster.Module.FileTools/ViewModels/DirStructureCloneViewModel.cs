using System.Collections.ObjectModel;
using ArchiveMaster.Basic;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public partial class
    DirStructureCloneViewModel(DirStructureCloneConfig config)
    : TwoStepViewModelBase<DirStructureCloneUtility, DirStructureCloneConfig>(config)
{
    [ObservableProperty]
    private BulkObservableCollection<SimpleFileInfo> treeFiles ;

    protected override Task OnInitializedAsync()
    {
        var files = new BulkObservableCollection<SimpleFileInfo>();
        files.AddRange(Utility.RootDir.Subs);
        TreeFiles = files;
        return base.OnInitializedAsync();
    }


    protected override void OnReset()
    {
        TreeFiles = null;
    }
}