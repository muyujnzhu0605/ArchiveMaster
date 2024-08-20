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
public abstract partial class ViewModelBase<TUtility, TConfig> :ViewModelBase
    where TUtility : UtilityBase<TConfig>
    where TConfig : ConfigBase
{
    public ViewModelBase(TConfig config)
    {
        Config = config;
    }


    protected virtual TUtility Utility { get; private set; }

    public virtual TConfig Config { get; }


    protected virtual TUtility CreateUtility()
    {
        Utility = CreateUtilityImplement();

        if (Utility == null)
        {
            Utility = Services.Provider.GetService<TUtility>();
        }

        return Utility;
    }

    protected virtual TUtility CreateUtilityImplement()
    {
        return null;
    }

    protected virtual void DisposeUtility()
    {
        Utility = null;
    }
}