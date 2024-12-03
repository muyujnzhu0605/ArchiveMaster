using ArchiveMaster.ViewModels.FileSystem;

namespace ArchiveMaster.Helpers;

public static class FileEnumerateExtension
{
    public static EnumerationOptions GetEnumerationOptions(bool includingSubDirs = true)
    {
        return new EnumerationOptions()
        {
            IgnoreInaccessible = true,
            AttributesToSkip = 0,
            RecurseSubdirectories = includingSubDirs,
        };
    }

    public static IEnumerable<T> ApplyFilter<T>(this IEnumerable<T> source,
        CancellationToken cancellationToken, bool skipRecycleBin = true)
    {
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!skipRecycleBin)
            {
                yield return item;
            }
            else
            {
                string path = item switch
                {
                    string str => str,
                    FileSystemInfo fi => fi.FullName,
                    SimpleFileInfo sf => sf.Path,
                    _ => null
                };

                if (path == null || !path.Contains("$RECYCLE.BIN", StringComparison.OrdinalIgnoreCase))
                {
                    yield return item;
                }
            }
        }
    }

    public static List<SimpleFileInfo> GetSimpleFileInfos(this DirectoryInfo directoryInfo,
        CancellationToken cancellationToken = default)
    {
        return directoryInfo.EnumerateSimpleFileInfos(cancellationToken).ToList();
    }

    public static IEnumerable<SimpleFileInfo> EnumerateSimpleFileInfos(this DirectoryInfo directoryInfo,
        CancellationToken cancellationToken = default)
    {
        return directoryInfo.EnumerateFiles("*", GetEnumerationOptions())
            .ApplyFilter(cancellationToken)
            .Select(p => new SimpleFileInfo(p, directoryInfo.FullName));
    }
}