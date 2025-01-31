using System;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Configs;

namespace ArchiveMaster.Services
{
    public abstract class TwoStepServiceBase<TConfig>(AppConfig appConfig) : ServiceBase<TConfig>(appConfig)
        where TConfig : ConfigBase
    {
        public abstract Task ExecuteAsync(CancellationToken token = default);

        public abstract Task InitializeAsync(CancellationToken token = default);
    }
}