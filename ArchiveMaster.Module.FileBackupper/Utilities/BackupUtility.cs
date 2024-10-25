using System.ComponentModel;
using System.Diagnostics;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Helpers;
using ArchiveMaster.Models;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.Utilities;

public partial class BackupService
{
    class BackupUtility(BackupTask backupTask)
    {
        public BackupTask BackupTask { get; } = backupTask;

        public Task BackupAsync(SnapshotType type, CancellationToken cancellationToken = default)
        {
            return type switch
            {
                SnapshotType.Full => FullBackupAsync(false, cancellationToken),
                SnapshotType.VirtualFull => FullBackupAsync(true, cancellationToken),
                SnapshotType.Increment => IncrementalBackupAsync(cancellationToken),
                _ => throw new InvalidEnumArgumentException()
            };
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

        private async Task FullBackupAsync(bool isVirtual, CancellationToken cancellationToken = default)
        {
            if (BackupTask.Status is BackupTaskStatus.FullBackingUp or BackupTaskStatus.IncrementBackingUp)
            {
                throw new InvalidOperationException("任务正在备份中，无法进行备份");
            }

            await Task.Run(async () =>
            {
                await using var db = new DbService(BackupTask);
                try
                {
                    BackupTask.BeginBackup(true);
                    BackupSnapshotEntity snapshot = new BackupSnapshotEntity()
                    {
                        BeginTime = DateTime.Now,
                        Type = isVirtual ? SnapshotType.VirtualFull : SnapshotType.Full
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
                    await db.LogAsync(LogLevel.Information, "备份完成");
                }
                catch (OperationCanceledException)
                {
                    await db.LogAsync(LogLevel.Error, $"备份被中止");
                }
                catch (Exception ex)
                {
                    await db.LogAsync(LogLevel.Error, $"全量备份过程中出现错误：{ex.Message}", detail: ex.ToString());
                }
                finally
                {
                    BackupTask.EndBackup(false);
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

        private async Task IncrementalBackupAsync(CancellationToken cancellationToken = default)
        {
            if (BackupTask.Status is BackupTaskStatus.FullBackingUp or BackupTaskStatus.IncrementBackingUp)
            {
                throw new InvalidOperationException("任务正在备份中，无法进行备份");
            }

            await Task.Run(async () =>
            {
                await using var db = new DbService(BackupTask);
                try
                {
                    BackupTask.BeginBackup(false);
                    BackupSnapshotEntity snapshot = new BackupSnapshotEntity()
                    {
                        BeginTime = DateTime.Now,
                        Type = SnapshotType.Increment
                    };
                    db.Add(snapshot);
                    await db.SaveChangesAsync(cancellationToken);
                    await db.LogAsync(LogLevel.Information, "开始增量备份", snapshot);

                    var latestFiles = db.GetLatestFiles(snapshot).ToDictionary(p => p.RawFileRelativePath);
                    await db.LogAsync(LogLevel.Information, $"已获取数据库中当前镜像的最新文件集合，共{latestFiles.Count}个", snapshot);

                    var files = GetSourceFiles(cancellationToken);
                    await db.LogAsync(LogLevel.Information, $"完成枚举磁盘文件，共{files.Count}个", snapshot);

                    bool hasChanged = false; //是否有文件增删改
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
                                //await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}未发生改变", snapshot);
                            }
                            else //文件发生改变
                            {
                                hasChanged = true;
                                await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}已修改", snapshot);
                                await CreateNewBackupFile(db, snapshot, file, FileRecordType.Modified);
                            }

                            latestFiles.Remove(rawRelativeFilePath);
                        }
                        else //文件有新增
                        {
                            hasChanged = true;
                            await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}已新增", snapshot);
                            await CreateNewBackupFile(db, snapshot, file, FileRecordType.Created);
                        }
                    }

                    foreach (var deletingFilePath in latestFiles.Keys) //存在于数据库但不存在于磁盘中的文件，表示已删除
                    {
                        hasChanged = true;
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

                    if (!hasChanged)
                    {
                        //没有任何文件改变，那这个快照是没有意义的。但是因为日志关联了这个快照，所以不能直接删除，采用软删除。
                        snapshot.IsDeleted = true;
                        await db.LogAsync(LogLevel.Information, "没有文件改变，快照已软删除");
                    }

                    await db.SaveChangesAsync(cancellationToken);

                    await db.LogAsync(LogLevel.Information, "备份完成");
                }
                catch (OperationCanceledException)
                {
                    await db.LogAsync(LogLevel.Error, $"备份被中止");
                }
                catch (Exception ex)
                {
                    await db.LogAsync(LogLevel.Error, $"增量备份过程中出现错误：{ex.Message}", detail: ex.ToString());
                }
                finally
                {
                    BackupTask.EndBackup(false);
                }
            }, cancellationToken);
        }
    }
}