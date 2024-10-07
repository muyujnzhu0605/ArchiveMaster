using System.Diagnostics;
using ArchiveMaster.Configs;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ArchiveMaster.Models;

public class BackupperDbContext : DbContext
{
    private readonly string connectionString;

    public BackupperDbContext(BackupperTask task)
    {
        if (!Directory.Exists(task.BackupDir))
        {
            throw new ArgumentException("备份存放目录不存在");
        }

        string dbPath = Path.Combine(task.BackupDir, "db.sqlite");
        connectionString = $"Data Source={dbPath}";
    }

    public DbSet<BackupSnapshotEntity> Snapshots { get; set; }

    public DbSet<BackupFileEntity> Files { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BackupFileEntity>()
            .HasOne(b => b.Snapshot)
            .WithMany() // 这里根据你的需求决定，如果 `BackupSnapshotEntity` 也有对应的 BackupFiles，可以添加 WithMany(b => b.BackupFiles)
            .HasForeignKey(b => b.SnapshotId);
    }
}