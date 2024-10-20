using System.Diagnostics;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Helpers;
using ArchiveMaster.Models;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.Utilities;

public class BackupUtility(BackupTask backupTask)
{
    public BackupTask BackupTask { get; } = backupTask;

    public async Task FullBackupAsync(bool isVirtual, CancellationToken cancellationToken = default)
    {
        await Task.Run(async () =>
        {
            await using var db = new DbService(BackupTask);
            try
            {
                BackupSnapshotEntity snapshot = new BackupSnapshotEntity()
                {
                    StartTime = DateTime.Now,
                    IsFull = true,
                    IsVirtual = isVirtual
                };
                db.Add(snapshot);
                await db.SaveChangesAsync(cancellationToken);
                await db.LogAsync(LogLevel.Information, "开始全量备份", snapshot);

                var files = GetSourceFiles(cancellationToken);
                await db.LogAsync(LogLevel.Information, $"完成枚举磁盘文件，共{files.Count}个", snapshot);
               
                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);
                    await db.LogAsync(LogLevel.Information, $"开始备份{rawRelativeFilePath}", snapshot);

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
                    db.Add(physicalFile);

                    FileRecordEntity record = new FileRecordEntity()
                    {
                        PhysicalFile = physicalFile,
                        Snapshot = snapshot,
                        RawFileRelativePath = rawRelativeFilePath,
                        Type = FileRecordType.Created
                    };
                    db.Add(record);

                    await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}已备份", snapshot);
                }

                snapshot.EndTime = DateTime.Now;
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await db.LogAsync(LogLevel.Error, $"全量备份过程中出现错误：{ex.Message}", detail: ex.ToString());
            }
        }, cancellationToken);
    }

    private List<FileInfo> GetSourceFiles(CancellationToken cancellationToken)
    {
        BlackListHelper blacks = new BlackListHelper(BackupTask.BlackList, BackupTask.BlackListUseRegex);
        var files = new DirectoryInfo(BackupTask.SourceDir)
            .EnumerateFiles("*", OptionsHelper.GetEnumerationOptions())
            .WithCancellationToken(cancellationToken)
            .Where(p => blacks.IsNotInBlackList(p))
            .ToList();
        return files;
    }

    public async Task IncrementalBackupAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(async () =>
        {
            await using var db = new DbService(BackupTask);
            try
            {
                BackupSnapshotEntity snapshot = new BackupSnapshotEntity()
                {
                    StartTime = DateTime.Now,
                    IsFull = false,
                    IsVirtual = false
                };
                db.Add(snapshot);
                await db.SaveChangesAsync(cancellationToken);
                await db.LogAsync(LogLevel.Information, "开始增量备份", snapshot);

                var latestFiles = db.GetLatestFiles(snapshot).ToDictionary(p => p.RawFileRelativePath);
                await db.LogAsync(LogLevel.Information, $"已获取数据库中当前镜像的最新文件集合，共{latestFiles.Count}个", snapshot);

                var files = GetSourceFiles(cancellationToken);
                await db.LogAsync(LogLevel.Information, $"完成枚举磁盘文件，共{files.Count}个", snapshot);

                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);
                    await db.LogAsync(LogLevel.Information, $"开始备份{rawRelativeFilePath}", snapshot);

                    if (latestFiles.TryGetValue(rawRelativeFilePath, out var latestFile)) //存在数据库中
                    {
                        if (latestFile.PhysicalFile.Time == file.LastWriteTime &&
                            latestFile.PhysicalFile.Length == file.Length) //文件没有发生改变
                        {                   
                            await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}未发生改变", snapshot);
                        }
                        else //文件发生改变
                        {
                            await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}已修改", snapshot);
                            await CreateNewBackupFile(db, snapshot, file, FileRecordType.Modified);
                        }

                        latestFiles.Remove(rawRelativeFilePath);
                    }
                    else //文件有新增
                    {
                        await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}已新增", snapshot);
                        await CreateNewBackupFile(db, snapshot, file, FileRecordType.Created);
                    }
                }

                foreach (var deletingFilePath in latestFiles.Keys) //存在于数据库但不存在于磁盘中的文件，表示已删除
                {
                    await db.LogAsync(LogLevel.Information, $"文件{deletingFilePath}已删除", snapshot);
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
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await db.LogAsync(LogLevel.Error, $"增量备份过程中出现错误：{ex.Message}", detail: ex.ToString());
            }
        }, cancellationToken);
    }

    private async Task CreateNewBackupFile(DbService db, BackupSnapshotEntity snapshot,
        FileInfo file, FileRecordType recordType)
    {
        string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);

        var backupFileName = Guid.NewGuid().ToString("N");
        string backupFilePath = Path.Combine(BackupTask.BackupDir, backupFileName);
        var sha1 = await FileHashHelper.CopyAndComputeSha1Async(file.FullName, backupFilePath);
        var physicalFile = db.GetSameFile(file.LastWriteTime, file.Length, sha1);
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
            db.Add(physicalFile);
        }

        FileRecordEntity record = new FileRecordEntity()
        {
            PhysicalFile = physicalFile,
            Snapshot = snapshot,
            RawFileRelativePath = rawRelativeFilePath,
            Type = recordType
        };
        db.Add(record);
    }
}