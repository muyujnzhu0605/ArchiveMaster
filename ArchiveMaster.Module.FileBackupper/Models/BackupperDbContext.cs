using ArchiveMaster.Configs;
using Microsoft.EntityFrameworkCore;

namespace ArchiveMaster.Models;

public class BackupperDbContext : DbContext
{
    private readonly string connectionString;

    public BackupperDbContext(BackupTask task)
    {
        if (!Directory.Exists(task.BackupDir))
        {
            throw new ArgumentException("备份存放目录不存在");
        }

        string dbPath = Path.Combine(task.BackupDir, "db.sqlite");
        connectionString = $"Data Source={dbPath}";
    }

    public DbSet<BackupFileEntity> Files { get; set; }
    public DbSet<BackupLogEntity> Logs { get; set; }
    public DbSet<BackupSnapshotEntity> Snapshots { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BackupFileEntity>()
            .HasOne(b => b.Snapshot)
            .WithMany()
            .HasForeignKey(b => b.SnapshotId);
    }
}