using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster;

public static class Services
{
    public static IServiceCollection Builder { get; } = new ServiceCollection();
    public static IServiceProvider Provider { get; private set; }

    public static void BuildServiceProvider()
    {
        Provider = Builder.BuildServiceProvider();
    }
}