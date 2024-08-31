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
    private ObservableCollection<SimpleFileInfo> files;
        
    [ObservableProperty]
    private TreeDirInfo root;
    
    [ObservableProperty]
    private BulkObservableCollection<SimpleFileInfo> treeFiles ;

    protected override Task OnInitializedAsync()
    {
        Files = new ObservableCollection<SimpleFileInfo>(Utility.Files);
        Root = Utility.RootDir;
        TreeFiles = new BulkObservableCollection<SimpleFileInfo>() { Root };
        return base.OnInitializedAsync();
    }


    protected override void OnReset()
    {
        Files = null;
    }
}