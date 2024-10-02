using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Configs;
using Microsoft.Extensions.Hosting;

namespace ArchiveMaster;

public class AppLifetime(AppConfig config) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        config.Save();
        return Task.CompletedTask;
    }
}
