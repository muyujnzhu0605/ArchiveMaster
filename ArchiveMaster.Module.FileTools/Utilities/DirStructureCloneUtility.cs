using System.ComponentModel;
using System.Runtime.InteropServices;
using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using Avalonia.Controls.Platform;
using Microsoft.Win32.SafeHandles;

namespace ArchiveMaster.Utilities
{
    public class DirStructureCloneUtility(DirStructureCloneConfig config) : TwoStepUtilityBase
    {
        public IList<FileInfoWithStatus> Files { get; private set; }
        public override DirStructureCloneConfig Config { get; } = config;

        public override async Task ExecuteAsync(CancellationToken token)
        {
            int index = 0;
            await Task.Run(() =>
            {
                foreach (var file in Files)
                {
                    token.ThrowIfCancellationRequested();
                    string relativePath = Path.GetRelativePath(Config.SourceDir, file.Path);
                    string newPath = Path.Combine(Config.TargetDir, relativePath);
                    FileInfo newFile = new FileInfo(newPath);
                    if (!newFile.Directory.Exists)
                    {
                        newFile.Directory.Create();
                    }

                    index++;
                    NotifyProgressUpdate(Files.Count, index, $"正在创建：{relativePath}（{index}/{Files.Count}）");

                    try
                    {
                        using (FileStream fs = File.Create(newPath))
                        {
                            MarkAsSparseFile(fs.SafeFileHandle);
                            fs.SetLength(file.Length);
                            fs.Seek(-1, SeekOrigin.End);
                        }

                        File.SetLastWriteTime(newPath, File.GetLastWriteTime(file.Path));
                        file.Complete = true;
                    }
                    catch (Exception ex)
                    {
                        file.Message = ex.Message;
                    }
                }
            }, token);
        }

        public override async Task InitializeAsync(CancellationToken token )
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new NotSupportedException("仅支持Windows");
            }

            List<FileInfoWithStatus> files = new List<FileInfoWithStatus>();

            NotifyProgressUpdate(-1, 0, $"正在枚举文件");
            await Task.Run(() =>
            {
                var fileInfos = new DirectoryInfo(Config.SourceDir)
                    .EnumerateFiles("*", new EnumerationOptions()
                    {
                        IgnoreInaccessible = true,
                        AttributesToSkip = 0,
                        RecurseSubdirectories = true,
                    }).ToList();
                token.ThrowIfCancellationRequested();
                int index = 0;
                foreach (var file in fileInfos)
                {
                    index++;
                    token.ThrowIfCancellationRequested();
                    NotifyProgressUpdate(fileInfos.Count, index,
                        $"正在处理：{Path.GetRelativePath(Config.SourceDir, file.FullName)}（{index}/{fileInfos.Count}）");

                    files.Add(new FileInfoWithStatus()
                    {
                        Name = file.Name,
                        Path = file.FullName,
                        Time = file.LastWriteTime,
                        Length = file.Length,
                    });
                }
            }, token);
            Files = files;
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            int dwIoControlCode,
            IntPtr InBuffer,
            int nInBufferSize,
            IntPtr OutBuffer,
            int nOutBufferSize,
            ref int pBytesReturned,
            [In] ref NativeOverlapped lpOverlapped
        );

        private static void MarkAsSparseFile(SafeFileHandle fileHandle)
        {
            int bytesReturned = 0;
            NativeOverlapped lpOverlapped = new NativeOverlapped();
            bool result =
                DeviceIoControl(
                    fileHandle,
                    590020, //FSCTL_SET_SPARSE,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    0,
                    ref bytesReturned,
                    ref lpOverlapped);
            if (result == false)
                throw new Win32Exception();
        }
    }
}