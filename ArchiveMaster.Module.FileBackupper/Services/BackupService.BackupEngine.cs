using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Helpers;
using ArchiveMaster.Models;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.Services;

public partial class BackupService
{
    class BackupEngine(BackupTask backupTask)
    {
        public BackupTask BackupTask { get; } = backupTask;

        public async Task BackupAsync(SnapshotType type, CancellationToken cancellationToken = default)
        {
            if (BackupTask.Status is BackupTaskStatus.FullBackingUp or BackupTaskStatus.IncrementBackingUp)
            {
                throw new InvalidOperationException("任务正在备份中，无法进行备份");
            }

            bool succeed = false;

            await Task.Run(async () =>
            {
                await using var db = new DbService(BackupTask);
                BackupSnapshotEntity snapshot = new BackupSnapshotEntity()
                {
                    BeginTime = DateTime.Now,
                    Type = type
                };

                try
                {
                    BackupTask.BeginBackup(type);

                    db.Add(snapshot);
                    await db.SaveChangesAsync(cancellationToken);
                    await db.LogAsync(LogLevel.Information, $"开始备份，模式：{type}", snapshot);

                    var files = GetSourceFiles(cancellationToken);
                    await db.LogAsync(LogLevel.Information, $"完成枚举磁盘文件，共{files.Count}个", snapshot);

                    if (type is SnapshotType.Increment)
                    {
                        await HandleIncrementalBackupAsync(db, snapshot, files, cancellationToken);
                    }
                    else
                    {
                        await HandleFullBackupAsync(db, snapshot, files, type is SnapshotType.VirtualFull,
                            cancellationToken);
                    }

                    snapshot.EndTime = DateTime.Now;
                    await db.SaveChangesAsync(cancellationToken);
                    await db.LogAsync(LogLevel.Information, "备份完成", snapshot);
                    succeed = true;
                }
                catch (OperationCanceledException)
                {
                    await db.LogAsync(LogLevel.Error, $"备份被中止", snapshot);
                }
                catch (Exception ex)
                {
                    await db.LogAsync(LogLevel.Error, $"备份过程中出现错误：{ex.Message}", snapshot, ex.ToString());
                    throw;
                }
                finally
                {
                    BackupTask.EndBackup(type is SnapshotType.Full or SnapshotType.VirtualFull, succeed);
                }
            }, cancellationToken);
        }

        private async Task CreateNewBackupFileAsync(DbService db, BackupSnapshotEntity snapshot,
            FileInfo file, FileRecordType recordType, bool isVirtual, CancellationToken cancellationToken)
        {
            if (recordType == FileRecordType.Deleted)
            {
                throw new ArgumentException("不支持删除类型", nameof(recordType));
            }

            string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);


            BackupFileEntity dbFile = new BackupFileEntity()
            {
                Snapshot = snapshot,
                RawFileRelativePath = rawRelativeFilePath,
                Type = recordType,
                Length = file.Length,
                Time = file.LastWriteTime,
            };

            if (!isVirtual)
            {
                dbFile.BackupFileName = Guid.NewGuid().ToString("N");
                string backupFilePath = Path.Combine(BackupTask.BackupDir, dbFile.BackupFileName);
                dbFile.Hash =
                    await FileHashHelper.CopyAndComputeSha1Async(file.FullName, backupFilePath, cancellationToken);

                var existedFile = db.GetSameFile(file.LastWriteTime, file.Length, dbFile.Hash);
                if (existedFile != null) //已经存在一样的物理文件了，那就把刚刚备份的文件给删掉
                {
                    await db.LogAsync(LogLevel.Information,
                        $"文件{rawRelativeFilePath}找到了已经存在的相同物理文件{dbFile.BackupFileName}", snapshot);
                    File.Delete(backupFilePath);
                    dbFile.BackupFileName = existedFile.BackupFileName;
                }
            }

            db.Add(dbFile);
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

        private async Task HandleFullBackupAsync(DbService db, BackupSnapshotEntity snapshot, List<FileInfo> files,
            bool isVirtualFull, CancellationToken cancellationToken)
        {
            foreach (var file in files)
            {
// #if DEBUG
//                 await Task.Delay(1000, cancellationToken);
// #endif
                cancellationToken.ThrowIfCancellationRequested();
                string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);
                try
                {
                    snapshot.CreatedFileCount++;
                    await CreateNewBackupFileAsync(db, snapshot, file, FileRecordType.Created, isVirtualFull,
                        cancellationToken);

                    await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}已备份", snapshot);
                }
                catch (IOException ex)
                {
                    await db.LogAsync(LogLevel.Error, $"文件{rawRelativeFilePath}备份失败", snapshot, ex.ToString());
                }
            }
        }

        private async Task HandleIncrementalBackupAsync(DbService db, BackupSnapshotEntity snapshot,
            List<FileInfo> files, CancellationToken cancellationToken)
        {
            var latestFiles = (await db.GetLatestFilesAsync(snapshot)).ToDictionary(p => p.RawFileRelativePath);
            await db.LogAsync(LogLevel.Information, $"已获取数据库中当前镜像的最新文件集合，共{latestFiles.Count}个", snapshot);

            foreach (var file in files)
            {
// #if DEBUG
//                 await Task.Delay(1000, cancellationToken);
// #endif
                cancellationToken.ThrowIfCancellationRequested();
                string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);
                try
                {
                    if (latestFiles.TryGetValue(rawRelativeFilePath, out var latestFile))
                    {
                        if (latestFile.Time != file.LastWriteTime || latestFile.Length != file.Length)
                        {
                            snapshot.ModifiedFileCount++;
                            await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}已修改", snapshot);
                            await CreateNewBackupFileAsync(db, snapshot, file, FileRecordType.Modified, false,
                                cancellationToken);
                        }

                        latestFiles.Remove(rawRelativeFilePath);
                    }
                    else
                    {
                        snapshot.CreatedFileCount++;
                        await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}已新增", snapshot);
                        await CreateNewBackupFileAsync(db, snapshot, file, FileRecordType.Created, false,
                            cancellationToken);
                    }
                }
                catch (IOException ex)
                {
                    await db.LogAsync(LogLevel.Error, $"文件{rawRelativeFilePath}备份失败", snapshot, ex.ToString());
                }
            }

            foreach (var deletingFilePath in latestFiles.Keys)
            {
                snapshot.DeletedFileCount++;
                await db.LogAsync(LogLevel.Information, $"文件{deletingFilePath}已删除", snapshot);
                BackupFileEntity record = new BackupFileEntity()
                {
                    Snapshot = snapshot,
                    RawFileRelativePath = deletingFilePath,
                    Type = FileRecordType.Deleted
                };
                db.Add(record);
            }

            if (snapshot.IsEmpty())
            {
                await db.LogAsync(LogLevel.Information, "没有文件改变");
            }
        }
    }
}