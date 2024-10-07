using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isWorking = false;

    public virtual void OnEnter()
    {
        
    }
}