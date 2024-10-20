using System.Diagnostics;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Helpers;
using ArchiveMaster.Models;

namespace ArchiveMaster.Utilities;

public class BackupUtility(BackupTask backupTask)
{
    private bool initialized = false;
    public BackupTask BackupTask { get; } = backupTask;

    public async Task FullBackupAsync(bool isVirtual, CancellationToken cancellationToken = default)
    {
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
                
                Debug.WriteLine($"文件{rawRelativeFilePath}已备份");
            }

            snapshot.EndTime = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    public async Task IncrementalBackupAsync(CancellationToken cancellationToken = default)
    {
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

            var latestFiles = RestoreUtility.GetLatestFiles(db, snapshot).ToDictionary(p => p.RawFileRelativePath);

            var files = new DirectoryInfo(BackupTask.SourceDir)
                .EnumerateFiles("*", OptionsHelper.GetEnumerationOptions())
                .WithCancellationToken(cancellationToken)
                .Where(p => blacks.IsNotInBlackList(p))
                .ToList();
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);

                if (latestFiles.TryGetValue(rawRelativeFilePath, out var latestFile)) //存在数据库中
                {
                    if (latestFile.PhysicalFile.Time == file.LastWriteTime &&
                        latestFile.PhysicalFile.Length == file.Length) //文件没有发生改变
                    {
                    }
                    else //文件发生改变
                    {
                        Debug.WriteLine($"文件{rawRelativeFilePath}发生改变");
                        await CreateNewBackupFile(db, snapshot, file, FileRecordType.Modified);
                    }

                    latestFiles.Remove(rawRelativeFilePath);
                }
                else //文件有新增
                {
                    Debug.WriteLine($"文件{rawRelativeFilePath}新增");
                    await CreateNewBackupFile(db, snapshot, file, FileRecordType.Created);
                }
            }

            foreach (var deletingFilePath in latestFiles.Keys) //存在于数据库但不存在于磁盘中的文件，表示已删除
            {
                Debug.WriteLine($"文件{deletingFilePath}被删除");
                FileRecordEntity record = new FileRecordEntity()
                {
                    Snapshot = snapshot,
                    RawFileRelativePath = deletingFilePath,
                    Type = FileRecordType.Deleted
                };
                db.Add(record);
            }
            
            snapshot.EndTime = DateTime.Now;
            await db.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    private async Task CreateNewBackupFile(BackupperDbContext db, BackupSnapshotEntity snapshot,
        FileInfo file, FileRecordType recordType)
    {
        string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);

        var backupFileName = Guid.NewGuid().ToString("N");
        string backupFilePath = Path.Combine(BackupTask.BackupDir, backupFileName);
        var sha1 = await FileHashHelper.CopyAndComputeSha1Async(file.FullName, backupFilePath);
        var physicalFile = db.Files
            .Where(p => p.Hash == sha1)
            .Where(p => p.Time == file.LastWriteTime)
            .Where(p => p.Length == file.Length)
            .FirstOrDefault(p => true);
        if (physicalFile != null) //已经存在一样的物理文件了，那就把刚刚备份的文件给删掉
        {
            File.Delete(backupFilePath);
        }
        else //没有相同的物理备份文件
        {
            physicalFile = new PhysicalFileEntity
            {
                FileName = backupFileName,
                Hash = sha1,
                Length = file.Length,
                Time = file.LastWriteTime,
            };
            db.Files.Add(physicalFile);
        }

        FileRecordEntity record = new FileRecordEntity()
        {
            PhysicalFile = physicalFile,
            Snapshot = snapshot,
            RawFileRelativePath = rawRelativeFilePath,
            Type = recordType
        };
        db.Records.Add(record);
    }

    private void Initialize(BackupperDbContext db)
    {
        if (!initialized)
        {
            db.Database.EnsureCreated();
            initialized = true;
        }
    }
}