using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArchiveMaster.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
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