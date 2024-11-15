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
            RegexOptions regexOptions = OperatingSystem.IsWindows() ? RegexOptions.IgnoreCase : RegexOptions.None;
            rIncludeFiles = string.IsNullOrWhiteSpace(filter.IncludeFiles)
                ? null
                : new Regex(filter.IncludeFiles, regexOptions);
            rIncludeFolders = string.IsNullOrWhiteSpace(filter.IncludeFolders)
                ? null
                : new Regex(filter.IncludeFolders, regexOptions);
            rIncludePaths = string.IsNullOrWhiteSpace(filter.IncludePaths)
                ? null
                : new Regex(filter.IncludePaths, regexOptions);
            rExcludeFiles = string.IsNullOrWhiteSpace(filter.ExcludeFiles)
                ? null
                : new Regex(filter.ExcludeFiles, regexOptions);
            rExcludeFolders = string.IsNullOrWhiteSpace(filter.ExcludeFolders)
                ? null
                : new Regex(filter.ExcludeFolders, regexOptions);
            rExcludePaths = string.IsNullOrWhiteSpace(filter.ExcludePaths)
                ? null
                : new Regex(filter.ExcludePaths, regexOptions);
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
        bool result = IsMatched(path.Replace('\\', '/'), Path.GetFileName(Path.GetDirectoryName(path)),
            Path.GetFileName(path));
        Console.WriteLine($"文件{path}筛选结果：{(result ? "通过" : "拦下")}");
        return result;
#else
        return IsMatched(path.Replace('\\','/'), Path.GetFileName(Path.GetDirectoryName(path)), Path.GetFileName(path));
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

    /// <summary>
    /// 支持通配符（*表示0个或多个任意字符，?表示1个任意字符）的字符串匹配
    /// </summary>
    /// <param name="text">字符串</param>
    /// <param name="pattern">匹配模式</param>
    /// <param name="contains">是否为包含模式。包含模式时，字符串包含pattern则为真，否则需要完全匹配</param>
    /// <returns></returns>
    public static bool IsMatchedByPattern(string text, string pattern, bool contains)
    {
        if (contains)
        {
            pattern = $"*{pattern.Trim('*')}*";
        }

        // 判断是否在 Windows 系统上运行
        if (OperatingSystem.IsWindows())
        {
            text = text.ToLower(); // 转换文本为小写
            pattern = pattern.ToLower(); // 转换模式为小写
        }

        int textLen = text.Length; // 文本长度
        int patternLen = pattern.Length; // 模式长度

        bool[] dp = new bool[patternLen + 1]; // 创建一维 DP 数组
        dp[0] = true; // 空模式与空文本匹配

        // 初始化 DP 数组：处理模式以 '*' 开头的情况
        for (int j = 1; j <= patternLen; j++)
        {
            if (pattern[j - 1] == '*') // 如果模式字符是 '*'，则当前 dp[j] 可以继承 dp[j-1]
            {
                dp[j] = dp[j - 1];
            }
        }

        // 逐字符遍历文本和模式，更新 DP 数组
        for (int i = 1; i <= textLen; i++)
        {
            bool prev = dp[0]; // 存储 dp[i-1][j-1]（上一行的状态）
            dp[0] = false; // dp[i][0] 表示文本非空时无法与空模式匹配

            for (int j = 1; j <= patternLen; j++)
            {
                bool temp = dp[j]; // 暂存 dp[i-1][j]，以备下次迭代使用

                if (pattern[j - 1] == text[i - 1] || pattern[j - 1] == '?') // 模式字符匹配文本字符或模式为 '?'
                {
                    dp[j] = prev; // 更新 dp[i][j] = dp[i-1][j-1]
                }
                else if (pattern[j - 1] == '*') // 模式字符为 '*'，表示匹配任意数量字符
                {
                    dp[j] = dp[j] || dp[j - 1]; // dp[i][j] = dp[i-1][j] 或 dp[i][j-1]
                }
                else
                {
                    dp[j] = false; // 模式与文本字符不匹配
                }

                prev = temp; // 更新 prev 为 dp[i-1][j]，用于下一次迭代
            }
        }

        return dp[patternLen]; // 返回最终结果，表示文本是否与模式完全匹配
    }

    private bool IsMatched(string path, string folder, string name)
    {
        bool MatchesRegex(Regex regex, string input)
        {
            return regex != null && regex.IsMatch(input);
        }

        if (useRegex)
        {
            if (path != null)
            {
                if (MatchesRegex(rExcludePaths, path)) return false;
                if (MatchesRegex(rIncludePaths, path)) return false;
            }

            if (folder != null)
            {
                if (MatchesRegex(rExcludeFolders, folder)) return false;
                if (MatchesRegex(rIncludeFolders, folder)) return false;
            }

            if (name != null)
            {
                if (MatchesRegex(rExcludeFiles, name)) return false;
                if (MatchesRegex(rIncludeFiles, name)) return false;
            }
        }
        else
        {
            if (path != null)
            {
                if (excludePaths.Any(p => IsMatchedByPattern(path, p, true))) return false;
                if (includePaths.All(p => !IsMatchedByPattern(path, p, false))) return false;
            }

            if (folder != null)
            {
                if (excludeFolders.Any(f => IsMatchedByPattern(folder, f, true))) return false;
                if (includeFolders.All(f => !IsMatchedByPattern(folder, f, false))) return false;
            }

            if (name != null)
            {
                if (excludeFiles.Any(f => IsMatchedByPattern(name, f, true))) return false;
                if (includeFiles.All(f => !IsMatchedByPattern(name, f, false))) return false;
            }
        }

        return true;
    }
}