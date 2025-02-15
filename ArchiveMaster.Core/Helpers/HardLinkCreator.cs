using System.Runtime.InteropServices;

namespace ArchiveMaster.Helpers;

public static class HardLinkCreator
{
    // Windows API for creating hard links
    [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName,
        IntPtr lpSecurityAttributes);

    // Linux and macOS API for creating hard links
    [DllImport("libc", SetLastError = true)]
    private static extern int link(string oldpath, string newpath);

    public static void CreateHardLink(string linkPath, string sourcePath)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException(sourcePath);
        }

        if (File.Exists(linkPath))
        {
            File.Delete(linkPath);
        }

        if (Path.GetPathRoot(linkPath) != Path.GetPathRoot(sourcePath))
        {
            throw new IOException("硬链接的两者必须在同一个分区中");
        }

        bool success;
        if (OperatingSystem.IsWindows())
        {
            success = CreateHardLink(linkPath, sourcePath, IntPtr.Zero);
            if (!success)
            {
                throw new Exception($"未知错误，无法创建硬链接：" + Marshal.GetLastWin32Error());
            }
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            int result = link(sourcePath, linkPath);
            if (result != 0)
            {
                throw new Exception($"未知错误，无法创建硬链接：" + Marshal.GetLastWin32Error());
            }
        }
        else
        {
            throw new PlatformNotSupportedException("不支持的操作系统");
        }
    }
}