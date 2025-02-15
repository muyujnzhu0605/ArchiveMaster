using System.Text.RegularExpressions;
using ArchiveMaster.ViewModels.FileSystem;

namespace ArchiveMaster.Helpers;

public class FilePlaceholderReplacer
{
    private const string DateTimeRegexString = @"(?<arg>[a-zA-Z\-]+)";
    private const string SubStringRegexString = @"(?<Direction>Left|Right)-(?<From>[0-9]+)-(?<Count>[0-9]+)";

    private static readonly Dictionary<string, Func<SimpleFileInfo, string, string>> fileAttributesDic;

    private string[] replacePatterns;

    static FilePlaceholderReplacer()
    {
        fileAttributesDic = new Dictionary<string, Func<SimpleFileInfo, string, string>>
        {
            // 文件名
            { "<FullName>", (item, arg) => item.Name },
            // 无扩展名的文件名
            { "<Name>", (item, arg) => Path.GetFileNameWithoutExtension(item.Name) },
            // 文件扩展名
            { "<Extension>", (item, arg) => Path.GetExtension(item.Name).TrimStart('.') },
            // 绝对路径
            { "<Path>", (item, arg) => item.Path },
            // 相对路径
            { "<RelativePath>", (item, arg) => item.RelativePath },
            // 无扩展名的文件名截取
            { "<Name-" + SubStringRegexString + ">", HandleSubstringPattern },
            // 文件大小
            { "<Size>", (item, arg) => item is SimpleFileInfo f ? f.Length.ToString() : "0" },
            // 文件夹名
            { "<Directory>", (item, arg) => Path.GetDirectoryName(item.Path) },
            // 创建时间
            { "<CreatTime-" + DateTimeRegexString + ">", HandleDateTimePattern(File.GetCreationTime) },
            // 创建时间UTC
            { "<CreatTimeUtc-" + DateTimeRegexString + ">", HandleDateTimePattern(File.GetCreationTimeUtc) },
            // 上次访问时间
            { "<LastAccessTime-" + DateTimeRegexString + ">", HandleDateTimePattern(File.GetLastAccessTime) },
            // 上次访问时间UTC
            { "<LastAccessTimeUtc-" + DateTimeRegexString + ">", HandleDateTimePattern(File.GetLastAccessTimeUtc) },
            // 修改时间
            {
                "<LastWriteTime-" + DateTimeRegexString + ">",
                (item, arg) => item.Time.ToString(ExtractDateTimeFormat(arg))
            },
            // 修改时间UTC
            {
                "<LastWriteTimeUtc-" + DateTimeRegexString + ">",
                (item, arg) => item.Time.ToString(ExtractDateTimeFormat(arg))
            }
        };
    }

    public FilePlaceholderReplacer(string template)
    {
        Template = template;
        PreprocessReplacePattern();
        if (replacePatterns.Length > 1 || replacePatterns.Length == 1 && replacePatterns[0].StartsWith('<'))
        {
            HasPattern = true;
        }
    }

    public bool HasPattern { get; }

    public string Template { get; }

    /// <summary>
    /// 获取替换后的文件名
    /// </summary>
    public string GetTargetName(SimpleFileInfo file, Func<string, string> extraProcess = null)
    {
        if (!HasPattern)
        {
            return Template;
        }

        return string.Concat(replacePatterns.Select(p =>
        {
            if (!p.StartsWith('<') || !fileAttributesDic.TryGetValue(p, out var func))
            {
                return p;
            }

            var result = func(file, p);
            return extraProcess == null ? result : extraProcess(result);
        }));
    }

    /// <summary>
    /// 预处理模板，提取占位符
    /// </summary>
    private void PreprocessReplacePattern()
    {
        var patterns = new List<string>();
        int left = 0;
        int right = 0;
        bool inPlaceholder = false;

        while (right < Template.Length)
        {
            char currentChar = Template[right];
            if (currentChar == '<')
            {
                if (right > left && !inPlaceholder)
                {
                    patterns.Add(Template[left..right]);
                }

                inPlaceholder = true;
                left = right;
            }
            else if (currentChar == '>' && inPlaceholder)
            {
                patterns.Add(Template[left..(right + 1)]);
                left = right + 1;
                inPlaceholder = false;
            }

            right++;
        }

        if (left < right)
        {
            patterns.Add(Template[left..]);
        }

        replacePatterns = patterns.ToArray();
    }

    /// <summary>
    /// 处理子字符串截取模式
    /// </summary>
    private static string HandleSubstringPattern(SimpleFileInfo item, string pattern)
    {
        try
        {
            string name = Path.GetFileNameWithoutExtension(item.Name);
            var match = Regex.Match(pattern, @"<Name-(?<Direction>Left|Right)-(?<From>\d+)-(?<Count>\d+)>");
            string direction = match.Groups["Direction"].Value;
            int from = int.Parse(match.Groups["From"].Value);
            int count = int.Parse(match.Groups["Count"].Value);

            if (direction == "Left")
            {
                return from >= name.Length ? "" : name.Substring(from, Math.Min(count, name.Length - from));
            }
            else
            {
                int startIndex = Math.Max(0, name.Length - from - count);
                return name.Substring(startIndex, Math.Min(count, name.Length - startIndex));
            }
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// 处理日期时间模式
    /// </summary>
    private static Func<SimpleFileInfo, string, string> HandleDateTimePattern(Func<string, DateTime> dateTimeGetter)
    {
        return (item, pattern) =>
        {
            string format = ExtractDateTimeFormat(pattern);
            return dateTimeGetter(item.Path).ToString(format);
        };
    }

    /// <summary>
    /// 从模式中提取日期时间格式
    /// </summary>
    private static string ExtractDateTimeFormat(string pattern)
    {
        var match = Regex.Match(pattern, @"<.*-(?<arg>[a-zA-Z\-]+)>");
        return match.Groups["arg"].Value;
    }
}