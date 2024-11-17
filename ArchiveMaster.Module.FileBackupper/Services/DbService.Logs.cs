using System.Collections.Concurrent;
using System.Diagnostics;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using ArchiveMaster.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.Services;

public partial class DbService
{
    private readonly BackupperDbContext logDb;

    private readonly ConcurrentBag<BackupLogEntity> logs = new ConcurrentBag<BackupLogEntity>();

    public async Task<PagedList<BackupLogEntity>> GetLogsAsync(int? snapshotId = null, LogLevel? type = null,
        string searchText = null, (DateTime from, DateTime to)? timeRange = null, int pageIndex = 0, int pageSize = 0)
    {
        await InitializeAsync();

        IQueryable<BackupLogEntity> query = db.Logs;
        if (snapshotId.HasValue)
        {
            query = query.Where(p => p.SnapshotId == snapshotId.Value);
        }

        if (type.HasValue && type.Value is not LogLevel.None)
        {
            query = query.Where(p => p.Type == type.Value);
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(p => p.Message.Contains(searchText));
        }

        if (timeRange.HasValue)
        {
            query=query.Where(p=>p.Time>timeRange.Value.from && p.Time<timeRange.Value.to);
        }

        query = query.OrderBy(p => p.Time);

        int totalCount = await query.CountAsync();

        if (pageSize > 0)
        {
            query = query.Skip(pageIndex * pageSize).Take(pageSize);
        }

        return new PagedList<BackupLogEntity>(await query.ToListAsync(), pageIndex, pageSize, totalCount);
    }


    public async ValueTask LogAsync(LogLevel logLevel, string message, BackupSnapshotEntity snapshot = null,
        string detail = null, bool forceSave = false)
    {
        await InitializeAsync(default, logDb);
        BackupLogEntity log = new BackupLogEntity()
        {
            Message = message,
            Type = logLevel,
            SnapshotId = snapshot?.Id,
            Detail = detail
        };
        //Debug.WriteLine($"{DateTime.Now}\t\t{message}");
        logs.Add(log);
        if (forceSave || logs.Count >= 1000)
        {
            Debug.WriteLine("保存日志");
            logDb.Logs.AddRange(logs);
            logs.Clear();
            await logDb.SaveChangesAsync();
        }
    }
}