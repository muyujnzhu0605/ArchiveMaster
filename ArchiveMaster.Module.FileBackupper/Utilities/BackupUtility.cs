using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Helpers;
using ArchiveMaster.Models;

namespace ArchiveMaster.Utilities;

public class BackupUtility(BackupTask backupTask)
{
    public BackupTask BackupTask { get; } = backupTask;
    private bool initialized = false;

    private void Initialize(BackupperDbContext db)
    {
        if (!initialized)
        {
            db.Database.EnsureCreated();
            initialized = true;
        }
    }

    public async Task FullBackupAsync(bool isVirtual, CancellationToken cancellationToken = default)
    {
        if (!initialized)
        {
            throw new InvalidOperationException("还未初始化");
        }

        await Task.Run(async () =>
        {
            await using var db = new BackupperDbContext(BackupTask);
            Initialize(db);
            BlackListHelper blacks = new BlackListHelper(BackupTask.BlackList, BackupTask.BlackListUseRegex);
            BackupSnapshotEntity snapshot = new BackupSnapshotEntity()
            {
                StartTime = DateTime.Now,
                IsFull = true,
                IsVirtual = isVirtual
            };
            db.Snapshots.Add(snapshot);

            var files = new DirectoryInfo(BackupTask.SourceDir)
                .EnumerateFiles("*", OptionsHelper.GetEnumerationOptions())
                .WithCancellationToken(cancellationToken)
                .Where(p => blacks.IsNotInBlackList(p))
                .ToList();
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);

                string backupFileName = null;
                string sha1 = null;

                if (!isVirtual)
                {
                    backupFileName = isVirtual ? null : Guid.NewGuid().ToString("N");
                    string backupFilePath = Path.Combine(BackupTask.BackupDir, backupFileName);
                    sha1 = await FileHashHelper.CopyAndComputeSha1Async(file.FullName, backupFilePath);
                }

                PhysicalFileEntity physicalFile = new PhysicalFileEntity
                {
                    FileName = backupFileName,
                    Hash = sha1,
                    Length = file.Length,
                    Time = file.LastWriteTime,
                };
                db.Files.Add(physicalFile);

                FileRecordEntity record = new FileRecordEntity()
                {
                    PhysicalFile = physicalFile,
                    Snapshot = snapshot,
                    RawFileRelativePath = rawRelativeFilePath,
                    Type = FileRecordType.Created
                };
                db.Records.Add(record);
            }

            snapshot.EndTime = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task IncrementalBackupAsync(CancellationToken cancellationToken = default)
    {
        if (!initialized)
        {
            throw new InvalidOperationException("还未初始化");
        }

        await Task.Run(async () =>
        {
            await using var db = new BackupperDbContext(BackupTask);
            Initialize(db);

            BlackListHelper blacks = new BlackListHelper(BackupTask.BlackList, BackupTask.BlackListUseRegex);
            BackupSnapshotEntity snapshot = new BackupSnapshotEntity()
            {
                StartTime = DateTime.Now,
                IsFull = false,
                IsVirtual = false
            };
            db.Snapshots.Add(snapshot);
            throw new NotImplementedException();
            var files = new DirectoryInfo(BackupTask.SourceDir)
                .EnumerateFiles("*", OptionsHelper.GetEnumerationOptions())
                .WithCancellationToken(cancellationToken)
                .Where(p => blacks.IsNotInBlackList(p))
                .ToList();
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);

                FileRecordEntity record = new FileRecordEntity()
                {
                    Snapshot = snapshot,
                    RawFileRelativePath = rawRelativeFilePath,
                    Type = FileRecordType.Created
                };
                db.Records.Add(record);

                string backupFileName = Guid.NewGuid().ToString("N");
                string backupFilePath = Path.Combine(BackupTask.BackupDir, backupFileName);
                string sha1 = await FileHashHelper.CopyAndComputeSha1Async(file.FullName, backupFilePath);

                PhysicalFileEntity physicalFile = new PhysicalFileEntity
                {
                    FileName = backupFileName,
                    Hash = sha1,
                    Length = file.Length,
                    Time = file.LastWriteTime,
                };
                record.PhysicalFile = physicalFile;
                db.Files.Add(physicalFile);
            }

            await db.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }
}