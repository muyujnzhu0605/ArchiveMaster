using Microsoft.Extensions.Hosting;

namespace ArchiveMaster;

public interface IBackgroundService : IHostedService
{
    /// <summary>
    /// 查询后台服务是否启动
    /// </summary>
    /// <remarks>
    /// 此处的启动指的是用户态是否允许该后台服务运行，并非指程序上的是否启动
    /// </remarks>
    bool IsEnabled { get; }
}