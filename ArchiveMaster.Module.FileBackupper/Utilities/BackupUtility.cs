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


        private async Task CreateNewBackupFileAsync(DbService db, BackupSnapshotEntity snapshot,
            FileInfo file, FileRecordType recordType, CancellationToken cancellationToken)
        {
            string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);

            var backupFileName = Guid.NewGuid().ToString("N");
            string backupFilePath = Path.Combine(BackupTask.BackupDir, backupFileName);
            var sha1 = await FileHashHelper.CopyAndComputeSha1Async(file.FullName, backupFilePath, cancellationToken);
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


        public async Task BackupAsync(SnapshotType type, CancellationToken cancellationToken = default)
        {
            if (BackupTask.Status is BackupTaskStatus.FullBackingUp or BackupTaskStatus.IncrementBackingUp)
            {
                throw new InvalidOperationException("任务正在备份中，无法进行备份");
            }

            bool isIncremental = type == SnapshotType.Increment;
            bool isVirtualFull = type == SnapshotType.VirtualFull;

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
                    BackupTask.BeginBackup(isIncremental);

                    db.Add(snapshot);
                    await db.SaveChangesAsync(cancellationToken);
                    await db.LogAsync(LogLevel.Information, $"开始{(isIncremental ? "增量" : "全量")}备份", snapshot);

                    var files = GetSourceFiles(cancellationToken);
                    await db.LogAsync(LogLevel.Information, $"完成枚举磁盘文件，共{files.Count}个", snapshot);

                    if (isIncremental)
                    {
                        await HandleIncrementalBackupAsync(db, snapshot, files, cancellationToken);
                    }
                    else
                    {
                        await HandleFullBackupAsync(db, snapshot, files, isVirtualFull, cancellationToken);
                    }

                    snapshot.EndTime = DateTime.Now;
                    await db.SaveChangesAsync(cancellationToken);
                    await db.LogAsync(LogLevel.Information, "备份完成", snapshot);
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
                    BackupTask.EndBackup(false);
                }
            }, cancellationToken);
        }

        private async Task HandleFullBackupAsync(DbService db, BackupSnapshotEntity snapshot, List<FileInfo> files,
            bool isVirtualFull, CancellationToken cancellationToken)
        {
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);
                try
                {
                    await db.LogAsync(LogLevel.Information, $"开始备份{rawRelativeFilePath}", snapshot);

                    string backupFileName = isVirtualFull ? null : Guid.NewGuid().ToString("N");
                    string sha1 = null;

                    if (!isVirtualFull)
                    {
                        string backupFilePath = Path.Combine(BackupTask.BackupDir, backupFileName);
                        sha1 = await FileHashHelper.CopyAndComputeSha1Async(file.FullName, backupFilePath,
                            cancellationToken);
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
                catch (IOException ex)
                {
                    await db.LogAsync(LogLevel.Error, $"文件{rawRelativeFilePath}备份失败", snapshot, ex.ToString());
                }
            }
        }

        private async Task HandleIncrementalBackupAsync(DbService db, BackupSnapshotEntity snapshot,
            List<FileInfo> files, CancellationToken cancellationToken)
        {
            var latestFiles = db.GetLatestFiles(snapshot).ToDictionary(p => p.RawFileRelativePath);
            await db.LogAsync(LogLevel.Information, $"已获取数据库中当前镜像的最新文件集合，共{latestFiles.Count}个", snapshot);

            bool hasChanged = false;
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string rawRelativeFilePath = Path.GetRelativePath(BackupTask.SourceDir, file.FullName);
                try
                {
                    if (latestFiles.TryGetValue(rawRelativeFilePath, out var latestFile))
                    {
                        if (latestFile.PhysicalFile.Time != file.LastWriteTime ||
                            latestFile.PhysicalFile.Length != file.Length)
                        {
                            hasChanged = true;
                            await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}已修改", snapshot);
                            await CreateNewBackupFileAsync(db, snapshot, file, FileRecordType.Modified,
                                cancellationToken);
                        }

                        latestFiles.Remove(rawRelativeFilePath);
                    }
                    else
                    {
                        hasChanged = true;
                        await db.LogAsync(LogLevel.Information, $"文件{rawRelativeFilePath}已新增", snapshot);
                        await CreateNewBackupFileAsync(db, snapshot, file, FileRecordType.Created, cancellationToken);
                    }
                }
                catch (IOException ex)
                {
                    await db.LogAsync(LogLevel.Error, $"文件{rawRelativeFilePath}备份失败", snapshot, ex.ToString());
                }
            }

            foreach (var deletingFilePath in latestFiles.Keys)
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

            if (!hasChanged)
            {
                await db.LogAsync(LogLevel.Information, "没有文件改变");
            }
        }
    }
}