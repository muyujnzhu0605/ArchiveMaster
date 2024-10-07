using Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiveMaster;

public static class Services
{
    public static void Initialize(IServiceProvider services)
    {
        if (Provider != null)
        {
            throw new InvalidOperationException("已经经过初始化");
        }

        Provider = services ?? throw new ArgumentNullException();
    }

    public static IServiceProvider Provider { get; private set; }

    public static void AddViewAndViewModel<TView, TViewModel>(this IServiceCollection services)
        where TView : StyledElement, new()
        where TViewModel : class
    {
        services.AddTransient<TViewModel>();
        services.AddTransient(s => new TView { DataContext = s.GetRequiredService<TViewModel>() });
    }
}