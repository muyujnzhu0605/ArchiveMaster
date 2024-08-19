using ArchiveMaster.Configs;
using ArchiveMaster.Utilities;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster.ViewModels;

public abstract partial class ViewModelBase<TUtility, TConfig> : ObservableObject
    where TUtility : UtilityBase<TConfig>
    where TConfig : ConfigBase
{
    public ViewModelBase(TConfig config)
    {
        Config = config;
    }

    [ObservableProperty]
    private bool isWorking = false;

    protected virtual TUtility Utility { get; private set; }

    public virtual TConfig Config { get; }


    protected virtual TUtility CreateUtility()
    {
        Utility = Services.Provider.GetService<TUtility>();
        return Utility;
    }

    protected virtual void DisposeUtility()
    {
        Utility = null;
    }
}