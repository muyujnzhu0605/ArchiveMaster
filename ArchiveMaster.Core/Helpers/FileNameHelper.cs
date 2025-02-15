using ArchiveMaster.Configs;
using ArchiveMaster.Enums;

namespace ArchiveMaster.Helpers;

public static class FileNameHelper
{
    public static string GenerateUniquePath(string desiredPath, ISet<string> usedPaths,
        string suffixTemplate = " ({0})", int firstCounter = 2)
    {
        if (!usedPaths.Contains(desiredPath))
        {
            return desiredPath;
        }

        string dir = Path.GetDirectoryName(desiredPath);
        string name = Path.GetFileNameWithoutExtension(desiredPath);
        string ext = Path.GetExtension(desiredPath);

        int counter = firstCounter;
        string newPath;
        do
        {
            string suffix = string.Format(suffixTemplate, counter);
            newPath = Path.Combine(dir, $"{name}{suffix}{ext}");
            counter++;
        } while (usedPaths.Contains(newPath));

        return newPath;
    }

    public static StringComparer GetStringComparer()
    {
        switch (GlobalConfigs.Instance.FileNameCase)
        {
            case FilenameCasePolicy.Auto:
                if (OperatingSystem.IsWindows())
                {
                    return StringComparer.OrdinalIgnoreCase;
                }
                else if(OperatingSystem.IsLinux())
                {
                    return StringComparer.Ordinal;
                }
                return StringComparer.OrdinalIgnoreCase;
            case FilenameCasePolicy.Ignore:
                return  StringComparer.OrdinalIgnoreCase;
            case FilenameCasePolicy.Sensitive:
                return StringComparer.Ordinal;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}