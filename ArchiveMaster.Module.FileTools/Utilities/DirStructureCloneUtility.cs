using System.ComponentModel;
using System.Runtime.InteropServices;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.ViewModels;
using Avalonia.Controls.Platform;
using Microsoft.Win32.SafeHandles;

namespace ArchiveMaster.Utilities
{
    public class DirStructureCloneUtility(DirStructureCloneConfig config)
        : TwoStepUtilityBase<DirStructureCloneConfig>(config)
    {
        public IList<SimpleFileInfo> Files { get; private set; }

        public override Task ExecuteAsync(CancellationToken token)
        {
            return TryForFilesAsync(Files, (file, s) =>
            {
                string relativePath = Path.GetRelativePath(Config.SourceDir, file.Path);
                string newPath = Path.Combine(Config.TargetDir, relativePath);
                FileInfo newFile = new FileInfo(newPath);
                if (!newFile.Directory.Exists)
                {
                    newFile.Directory.Create();
                }

                NotifyMessage($"正在创建{s.GetFileNumberMessage()}：{relativePath}");


                using (FileStream fs = File.Create(newPath))
                {
                    MarkAsSparseFile(fs.SafeFileHandle);
                    fs.SetLength(file.Length);
                    fs.Seek(-1, SeekOrigin.End);
                }

                File.SetLastWriteTime(newPath, File.GetLastWriteTime(file.Path));
            }, token, FilesLoopOptions.Builder().AutoApplyFileLengthProgress().AutoApplyStatus().Build());
        }

        public override async Task InitializeAsync(CancellationToken token)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new NotSupportedException("仅支持Windows");
            }

            List<SimpleFileInfo> files = new List<SimpleFileInfo>();

            NotifyMessage("正在枚举文件");
            NotifyProgressIndeterminate();

            await Task.Run(() =>
            {
                var fileInfos = new DirectoryInfo(Config.SourceDir)
                    .EnumerateFiles("*", new EnumerationOptions()
                    {
                        IgnoreInaccessible = true,
                        AttributesToSkip = 0,
                        RecurseSubdirectories = true,
                    });

                TryForFiles(fileInfos.Select(p => new SimpleFileInfo(p, Config.SourceDir)), (file, s) =>
                {
                    NotifyMessage($"正在处理{s.GetFileNumberMessage()}：{file.RelativePath}");
                    files.Add(file);
                }, token, FilesLoopOptions.DoNothing());
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
            [In]
            ref NativeOverlapped lpOverlapped
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