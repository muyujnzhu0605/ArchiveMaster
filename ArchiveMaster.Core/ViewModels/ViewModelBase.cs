using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isWorking = false;
    
    protected virtual UtilityBase Utility { get; private set; }
    
    public abstract ConfigBase Config { get; }

    protected virtual T CreateUtility<T>() where T : UtilityBase
    {
        Utility = Activator.CreateInstance(typeof(T), Config) as T;
        return Utility as T;
    }

    protected virtual void DisposeUtility()
    {
        Utility = null;
    }
}