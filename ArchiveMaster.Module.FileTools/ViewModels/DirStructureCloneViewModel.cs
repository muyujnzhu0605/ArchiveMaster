using System.Collections.ObjectModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public partial class DirStructureCloneViewModel : TwoStepViewModelBase<DirStructureCloneUtility>
{
    [ObservableProperty]
    private ObservableCollection<SimpleFileInfo> files;
    public override DirStructureCloneConfig Config { get; } = AppConfig.Instance.Get<DirStructureCloneConfig>();

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