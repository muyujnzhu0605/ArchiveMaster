using System.Security.Cryptography;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Helpers;
using ArchiveMaster.Models;
using Microsoft.EntityFrameworkCore;

namespace ArchiveMaster.Utilities;

public class BackupUtility(BackupTask task)
{
    public BackupTask Task { get; } = task;
    private bool initialized = false;

    public async Task InitializeAsync()
    {
        await using var db = new BackupperDbContext(Task);
        await db.Database.EnsureCreatedAsync();
        initialized = true;
    }

    public async Task FullBackupAsync(bool isVirtual)
    {
        if (!initialized)
        {
            throw new InvalidOperationException("还未初始化");
        }

        await System.Threading.Tasks.Task.Run(async () =>
        {
            using var db = new BackupperDbContext(Task);
            BlackListHelper blacks = new BlackListHelper(Task.BlackList, Task.BlackListUseRegex);
            BackupSnapshotEntity snapshot = new BackupSnapshotEntity()
            {
                StartTime = DateTime.Now,
                IsFull = true,
                IsVirtual = isVirtual
            };
            db.Snapshots.Add(snapshot);

            var files = new DirectoryInfo(Task.SourceDir)
                .EnumerateFiles("*", OptionsHelper.GetEnumerationOptions())
                .Where(p => blacks.IsNotInBlackList(p))
                .ToList();
            foreach (var file in files)
            {
                string fileName = isVirtual ? null : Guid.NewGuid().ToString("N");
                string rawRelativeFilePath = Path.GetRelativePath(Task.SourceDir, file.FullName);
                string sha1;
                if (isVirtual)
                {
                    sha1 = await FileHashHelper.ComputeSha1Async(file.FullName);
                }
                else
                {
                    string backupFilePath = Path.Combine(Task.BackupDir, fileName);
                    sha1 = await FileHashHelper.CopyAndComputeSha1Async(file.FullName, backupFilePath);
                }

                PhysicalFileEntity physicalFile = new PhysicalFileEntity
                {
                    FileName = fileName,
                    Hash = sha1,
                    Length = file.Length,
                    Time = file.LastWriteTime,
                    RefCount = 1,
                    FullSnapshot = snapshot,
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

            await db.SaveChangesAsync();
        });
    }
}