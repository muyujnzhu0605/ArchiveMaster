using System;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Configs;

namespace ArchiveMaster.Services
{
    public abstract class TwoStepServiceBase<TConfig> : ServiceBase<TConfig> where TConfig : ConfigBase
    {
        public TwoStepServiceBase(TConfig config, AppConfig appConfig) : base(config, appConfig)
        {
        }

        public abstract Task ExecuteAsync(CancellationToken token = default);

        public abstract Task InitializeAsync(CancellationToken token = default);
    }
}