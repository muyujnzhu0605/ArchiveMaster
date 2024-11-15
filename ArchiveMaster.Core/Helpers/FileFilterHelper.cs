using System.Text.RegularExpressions;
using ArchiveMaster.Configs;
using ArchiveMaster.ViewModels;
using FzLib;
using SimpleFileInfo = ArchiveMaster.ViewModels.FileSystem.SimpleFileInfo;

namespace ArchiveMaster.Helpers;

public class FileFilterHelper
{
    private readonly string[] excludeFiles;

    private readonly string[] excludeFolders;

    private readonly string[] excludePaths;

    private readonly string[] includeFiles;

    private readonly string[] includeFolders;

    private readonly string[] includePaths;

    private readonly Regex rExcludeFiles;

    private readonly Regex rExcludeFolders;

    private readonly Regex rExcludePaths;

    private readonly Regex rIncludeFiles;

    private readonly Regex rIncludeFolders;

    private readonly Regex rIncludePaths;

    private readonly bool useRegex;

    public FileFilterHelper(FileFilterConfig filter)
    {
        useRegex = filter.UseRegex;
        if (filter.UseRegex)
        {
            rIncludeFiles = string.IsNullOrWhiteSpace(filter.IncludeFiles)
                ? null
                : new Regex(filter.IncludeFiles);
            rIncludeFolders = string.IsNullOrWhiteSpace(filter.IncludeFolders)
                ? null
                : new Regex(filter.IncludeFolders);
            rIncludePaths = string.IsNullOrWhiteSpace(filter.IncludePaths)
                ? null
                : new Regex(filter.IncludePaths);
            rExcludeFiles = string.IsNullOrWhiteSpace(filter.ExcludeFiles)
                ? null
                : new Regex(filter.ExcludeFiles);
            rExcludeFolders = string.IsNullOrWhiteSpace(filter.ExcludeFolders)
                ? null
                : new Regex(filter.ExcludeFolders);
            rExcludePaths = string.IsNullOrWhiteSpace(filter.ExcludePaths)
                ? null
                : new Regex(filter.ExcludePaths);
        }
        else
        {
            includeFiles = string.IsNullOrWhiteSpace(filter.IncludeFiles)
                ? []
                : filter.IncludeFiles.Split(Environment.NewLine);

            includeFolders = string.IsNullOrWhiteSpace(filter.IncludeFolders)
                ? []
                : filter.IncludeFolders.Split(Environment.NewLine);

            includePaths = string.IsNullOrWhiteSpace(filter.IncludePaths)
                ? []
                : filter.IncludePaths.Split(Environment.NewLine);

            excludeFiles = string.IsNullOrWhiteSpace(filter.ExcludeFiles)
                ? []
                : filter.ExcludeFiles.Split(Environment.NewLine);

            excludeFolders = string.IsNullOrWhiteSpace(filter.ExcludeFolders)
                ? []
                : filter.ExcludeFolders.Split(Environment.NewLine);

            excludePaths = string.IsNullOrWhiteSpace(filter.ExcludePaths)
                ? []
                : filter.ExcludePaths.Split(Environment.NewLine);
        }
    }

    public bool IsMatched(string path)
    {
#if DEBUG
        bool result = IsMatched(path, Path.GetDirectoryName(path), Path.GetFileName(path));
        Console.WriteLine($"文件{path}筛选结果：{(result ? "通过" : "拦下")}");
        return result;
#else
        return IsMatched(path, Path.GetDirectoryName(path), Path.GetFileName(path));
#endif
    }

    public bool IsMatched(SimpleFileInfo file)
    {
        return IsMatched(file.Path);
    }

    public bool IsMatched(FileInfo file)
    {
        return IsMatched(file.FullName);
    }

    private static bool IsMatchedByPattern(string text, string pattern)
    {
        int textLen = text.Length;
        int patternLen = pattern.Length;
        bool[,] dp = new bool[textLen + 1, patternLen + 1];

        // Initialize the DP table
        dp[0, 0] = true;

        // Handle patterns with leading '*'
        for (int j = 1; j <= patternLen; j++)
        {
            if (pattern[j - 1] == '*')
            {
                dp[0, j] = dp[0, j - 1];
            }
        }

        // Fill the DP table
        for (int i = 1; i <= textLen; i++)
        {
            for (int j = 1; j <= patternLen; j++)
            {
                if (pattern[j - 1] == text[i - 1] || pattern[j - 1] == '?')
                {
                    dp[i, j] = dp[i - 1, j - 1];
                }
                else if (pattern[j - 1] == '*')
                {
                    dp[i, j] = dp[i - 1, j] || dp[i, j - 1];
                }
            }
        }

        return dp[textLen, patternLen];
    }
    private bool IsMatched(string path, string folder, string name)
    {
        if (useRegex)
        {
            if (path != null)
            {
                if (rIncludePaths != null && !rIncludePaths.IsMatch(path))
                {
                    return false;
                }

                if (rExcludePaths != null && rExcludePaths.IsMatch(path))
                {
                    return false;
                }
            }

            if (folder != null)
            {
                if (rIncludeFolders != null && !rIncludeFolders.IsMatch(folder))
                {
                    return false;
                }

                if (rExcludeFolders != null && rExcludeFolders.IsMatch(folder))
                {
                    return false;
                }
            }

            if (name != null)
            {
                if (rIncludeFiles != null && !rIncludeFiles.IsMatch(name))
                {
                    return false;
                }

                if (rExcludeFiles != null && rExcludeFiles.IsMatch(name))
                {
                    return false;
                }
            }
        }
        else
        {
            if (path != null)
            {
                if (includePaths.Any(p => !IsMatchedByPattern(path, p)))
                {
                    return false;
                }

                if (excludePaths.Any(p => IsMatchedByPattern(path, p)))
                {
                    return false;
                }
            }

            if (folder != null)
            {
                if (includeFolders.Any(f => !IsMatchedByPattern(folder, f)))
                {
                    return false;
                }

                if (excludeFolders.Any(f => IsMatchedByPattern(folder, f)))
                {
                    return false;
                }
            }

            if (name != null)
            {
                if (includeFiles.Any(f => !IsMatchedByPattern(name, f)))
                {
                    return false;
                }

                if (excludeFiles.Any(f => IsMatchedByPattern(name, f)))
                {
                    return false;
                }
            }
        }

        return true;
    }
}