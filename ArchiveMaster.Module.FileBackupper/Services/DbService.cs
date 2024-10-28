using System.Collections.Concurrent;
using System.Diagnostics;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.Services;

public partial class DbService : IDisposable, IAsyncDisposable
{
    private static readonly HashSet<BackupTask> initializedTasks = new HashSet<BackupTask>();

    private readonly BackupperDbContext db;
    
    public DbService(BackupTask backupTask)
    {
        BackupTask = backupTask;
        db = new BackupperDbContext(backupTask);
        logDb = new BackupperDbContext(backupTask);
        Initialize();
    }

    public BackupTask BackupTask { get; }

    public void Add(object entity)
    {
        switch (entity)
        {
            case BackupSnapshotEntity snapshotEntity:
                db.Snapshots.Add(snapshotEntity);
                break;
            case BackupFileEntity file:
                db.Files.Add(file);
                break;
            case BackupLogEntity log:
                db.Logs.Add(log);
                break;
            default:
                db.Add(entity);
                break;
        }
    }

    public void Dispose()
    {
        if (!logs.IsEmpty)
        {
            Debug.WriteLine("保存日志");
            logDb.Logs.AddRange(logs);
            logs.Clear();
            logDb.SaveChanges();
        }

        db?.Dispose();
        logDb?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (!logs.IsEmpty)
        {
            Debug.WriteLine("保存日志");
            logDb.Logs.AddRange(logs);
            logs.Clear();
            await logDb.SaveChangesAsync();
        }

        if (db != null)
        {
            await db.DisposeAsync();
        }

        if (logDb != null)
        {
            await logDb.DisposeAsync();
        }
    }


    private void Initialize(DbContext db = null)
    {
        if (initializedTasks.Contains(BackupTask))
        {
            return;
        }

        db ??= this.db;
        initializedTasks.Add(BackupTask);
        db.Database.EnsureCreated();
    }

    private ValueTask InitializeAsync(CancellationToken cancellationToken = default, DbContext db = null)
    {
        if (initializedTasks.Contains(BackupTask))
        {
            return ValueTask.CompletedTask;
        }

        db ??= this.db;
        initializedTasks.Add(BackupTask);
        return new ValueTask(db.Database.EnsureCreatedAsync(cancellationToken));
    }    
    
    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}
