using System.Text.RegularExpressions;
using ArchiveMaster.ViewModels.FileSystem;

namespace ArchiveMaster.Helpers;

public partial class FilePlaceholderReplacer
{
    private const string DateTimeRegexString = @"(?<arg>[a-zA-Z\-]+)";
    private const string SubStringRegexString = @"(?<Direction>Left|Right)-(?<From>[0-9]+)-(?<Count>[0-9]+)";

    private string[] replacePatterns;

    public FilePlaceholderReplacer(string template)
    {
        Template = template;
        PreprocessReplacePattern();
        HasPattern = replacePatterns.Length > 1 || (replacePatterns.Length == 1 && replacePatterns[0].StartsWith('<'));
    }

    public bool HasPattern { get; }
    public string Template { get; }

    /// <summary>
    /// 获取替换后的文件名
    /// </summary>
    public string GetTargetName(SimpleFileInfo file, Func<string, string> extraProcess = null)
    {
        if (!HasPattern) return Template;

        return string.Concat(replacePatterns.Select(p =>
        {
            if (!p.StartsWith('<')) return p;

            var result = GetReplacementValue(file, p);
            return extraProcess != null ? extraProcess(result) : result;
        }));
    }

    /// <summary>
    /// 预处理模板，提取占位符
    /// </summary>
    private void PreprocessReplacePattern()
    {
        var patterns = new List<string>();
        int left = 0, right = 0;
        bool inPlaceholder = false;

        while (right < Template.Length)
        {
            if (Template[right] == '<')
            {
                if (right > left && !inPlaceholder)
                    patterns.Add(Template[left..right]);

                inPlaceholder = true;
                left = right;
            }
            else if (Template[right] == '>' && inPlaceholder)
            {
                patterns.Add(Template[left..(right + 1)]);
                left = right + 1;
                inPlaceholder = false;
            }

            right++;
        }

        if (left < right) patterns.Add(Template[left..]);
        replacePatterns = patterns.ToArray();
    }

    private static string GetReplacementValue(SimpleFileInfo item, string placeholder)
    {
        return placeholder switch
        {
            // 固定占位符
            "<NameExt>" => item.Name,
            "<Name>" => Path.GetFileNameWithoutExtension(item.Name),
            "<Ext>" => Path.GetExtension(item.Name).TrimStart('.'),
            "<Path>" => item.Path,
            "<RelPath>" => item.RelativePath,
            "<Len>" => item.Length.ToString(),
            "<DirPath>" => Path.GetDirectoryName(item.Path),
            "<DirRelPath>" => Path.GetDirectoryName(item.RelativePath),
            "<DirName>" => Path.GetFileName(Path.GetDirectoryName(item.RelativePath)),
            // 带参数的占位符（使用正则表达式匹配）
            _ when SubNameRegex().IsMatch(placeholder)
                => HandleSubstringPattern(item, placeholder),
            _ when CreateTimeRegex().IsMatch(placeholder)
                => HandleDateTimePattern(item, placeholder, File.GetCreationTime),
            _ when CreateTimeUtcRegex().IsMatch(placeholder)
                => HandleDateTimePattern(item, placeholder, File.GetCreationTimeUtc),
            _ when AccessTimeRegex().IsMatch(placeholder)
                => HandleDateTimePattern(item, placeholder, File.GetLastAccessTime),
            _ when AccessTimeUtcRegex().IsMatch(placeholder)
                => HandleDateTimePattern(item, placeholder, File.GetLastAccessTimeUtc),
            _ when WriteTimeRegex().IsMatch(placeholder)
                => HandleCustomDateTime(item, placeholder, isUtc: false),
            _ when WriteTimeUtcRegex().IsMatch(placeholder)
                => HandleCustomDateTime(item, placeholder, isUtc: true),
            // 默认情况
            _ => placeholder
        };
    }

    /// <summary>
    /// 处理子字符串截取模式
    /// </summary>
    private static string HandleSubstringPattern(SimpleFileInfo item, string placeholder)
    {
        try
        {
            var match = SubStringRegex().Match(placeholder);
            var direction = match.Groups["Direction"].Value;
            int from = int.Parse(match.Groups["From"].Value);
            int count = int.Parse(match.Groups["Count"].Value);

            var name = Path.GetFileNameWithoutExtension(item.Name);
            return direction switch
            {
                "Left" => from >= name.Length ? "" : name.Substring(from, Math.Min(count, name.Length - from)),
                "Right" => name.Substring(Math.Max(0, name.Length - from - count),
                    Math.Min(count, name.Length - Math.Max(0, name.Length - from - count))),
                _ => ""
            };
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// 处理日期时间模式（通用方法）
    /// </summary>
    private static string HandleDateTimePattern(SimpleFileInfo item, string placeholder,
        Func<string, DateTime> dateGetter)
    {
        var format = Regex.Match(placeholder, @"<.*-(?<arg>[a-zA-Z\-]+)>").Groups["arg"].Value;
        return dateGetter(item.Path).ToString(format);
    }

    /// <summary>
    /// 处理自定义的LastWriteTime模式
    /// </summary>
    private static string HandleCustomDateTime(SimpleFileInfo item, string placeholder, bool isUtc)
    {
        var format = DateTimeRegex().Match(placeholder).Groups["arg"].Value;
        return isUtc ? item.Time.ToUniversalTime().ToString(format) : item.Time.ToString(format);
    }

    [GeneratedRegex(@"<Name-(?<Direction>Left|Right)-(?<From>\d+)-(?<Count>\d+)>")]
    private static partial Regex SubStringRegex();

    [GeneratedRegex(@"<.*-(?<arg>[a-zA-Z\-]+)>")]
    private static partial Regex DateTimeRegex();
    [GeneratedRegex(@"^<Name-Left|Right-\d+-\d+>$")]
    private static partial Regex SubNameRegex();
    [GeneratedRegex(@"^<CreatTime-([a-zA-Z\-]+)>$")]
    private static partial Regex CreateTimeRegex();
    [GeneratedRegex(@"^<CreatTimeUtc-([a-zA-Z\-]+)>$")]
    private static partial Regex CreateTimeUtcRegex();
    [GeneratedRegex(@"^<LastAccessTime-([a-zA-Z\-]+)>$")]
    private static partial Regex AccessTimeRegex();
    [GeneratedRegex(@"^<LastAccessTimeUtc-([a-zA-Z\-]+)>$")]
    private static partial Regex AccessTimeUtcRegex();
    [GeneratedRegex(@"^<LastWriteTime-([a-zA-Z\-]+)>$")]
    private static partial Regex WriteTimeRegex();
    [GeneratedRegex(@"^<LastWriteTimeUtc-([a-zA-Z\-]+)>$")]
    private static partial Regex WriteTimeUtcRegex();
}