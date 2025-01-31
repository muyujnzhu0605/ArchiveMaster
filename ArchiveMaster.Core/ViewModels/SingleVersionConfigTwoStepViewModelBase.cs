using ArchiveMaster.Configs;
using ArchiveMaster.Services;

namespace ArchiveMaster.ViewModels;

public abstract class
    SingleVersionConfigTwoStepViewModelBase<TService, TConfig>(AppConfig appConfig)
    : TwoStepViewModelBase<TService, TConfig>(appConfig.GetConfig<TConfig>(typeof(TConfig).Name), appConfig)
    where TService : TwoStepServiceBase<TConfig>
    where TConfig : ConfigBase, new();