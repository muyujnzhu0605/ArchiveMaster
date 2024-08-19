using System.Collections.ObjectModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public partial class
    DirStructureCloneViewModel(DirStructureCloneUtility utility, DirStructureCloneConfig config)
    : TwoStepViewModelBase<DirStructureCloneUtility, DirStructureCloneConfig>(utility,
        config)
{
    [ObservableProperty]
    private ObservableCollection<SimpleFileInfo> files;

    protected override Task OnInitializedAsync()
    {
        Files = new ObservableCollection<SimpleFileInfo>(Utility.Files);
        return base.OnInitializedAsync();
    }


    protected override void OnReset()
    {
        Files = null;
    }
}