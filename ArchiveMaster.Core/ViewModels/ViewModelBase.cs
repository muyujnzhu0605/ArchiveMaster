using System.ComponentModel;
using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    public static ViewModelBase Current { get; private set; }
    
    [ObservableProperty]
    private bool isWorking = false;

    public virtual void OnEnter()
    {
        Current = this;
    }

    public virtual Task OnExitAsync(CancelEventArgs args)
    {
        Current = null;
        return Task.CompletedTask;
    }
}