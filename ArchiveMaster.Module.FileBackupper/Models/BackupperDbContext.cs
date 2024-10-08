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

    public DbSet<BackupSnapshotEntity> Snapshots { get; set; }

    public DbSet<FileRecordEntity> Records { get; set; }

    public DbSet<PhysicalFileEntity> Files { get; set; }

    public DbSet<BackupLogEntity> Logs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileRecordEntity>()
            .HasOne(b => b.Snapshot)
            .WithMany()
            .HasForeignKey(b => b.SnapshotId);
        modelBuilder.Entity<FileRecordEntity>()
            .HasOne(b => b.PhysicalFile)
            .WithMany()
            .HasForeignKey(b => b.PhysicalFileId);
        modelBuilder.Entity<BackupLogEntity>()
            .HasOne(b => b.Snapshot)
            .WithMany()
            .HasForeignKey(b => b.SnapshotId);
    }
}