using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster;

public static class Services
{
    //private static IServiceProvider provider;

    //public static void Initialize(IServiceCollection services)
    //{
    //    if (Builder != null)
    //    {
    //        throw new InvalidOperationException("已经经过初始化");
    //    }
    //    Builder = services ?? throw new ArgumentNullException();
    //}
    //public static IServiceCollection Builder { get; private set; }
    //public static IServiceProvider Provider
    //{
    //    get => provider??throw new InvalidOperationException("还未初始化");
    //    private set => provider = value;
    //}

    //public static void BuildServiceProvider()
    //{
    //    Provider = Builder.BuildServiceProvider();
    //}
    public static void Initialize(IServiceProvider services)
    {
        if (Provider != null)
        {
            throw new InvalidOperationException("已经经过初始化");
        }
        Provider = services ?? throw new ArgumentNullException();
    }
    public static IServiceProvider Provider { get; private set; }

}