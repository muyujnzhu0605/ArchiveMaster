using System.Collections;
using System.Text.RegularExpressions;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.ViewModels;
using Avalonia.Media;
using FzLib.Avalonia.Converters;
using FzLib.Program;

namespace ArchiveMaster.Utilities;

public class RenameUtility(RenameConfig config) : TwoStepUtilityBase
{
    public static readonly Dictionary<string, Func<SimpleFileInfo, string, string>> fileAttributesDic =
        new Dictionary<string, Func<SimpleFileInfo, string, string>>()
        {
            //文件名
            { "<Name>", (item, arg) => item.Name },
            //无扩展名的文件名
            { "<NameWithoutExtension>", (item, arg) => Path.GetFileNameWithoutExtension(item.Name) },
            //文件扩展名
            {
                "<NameExtension>", (item, arg) =>
                {
                    string extension = Path.GetExtension(item.Name);
                    return extension == "" ? "" : extension.Replace(".", "");
                }
            },
            //无扩展名的文件名截取
            {
                "<NameWithoutExtension-" + SubStringRegexString + ">", (item, arg) =>
                {
                    try
                    {
                        string extension = Path.GetExtension(item.Name);
                        string name = extension == "" ? item.Name : item.Name.Replace(extension, "");
                        Match match = Regex.Match(arg, "<NameWithoutExtension-" + SubStringRegexString + ">");
                        string direction = match.Groups["Direction"].Value;
                        int from = int.Parse(match.Groups["From"].Value);
                        int count = int.Parse(match.Groups["Count"].Value);
                        int length = name.Length;
                        if (direction == "Left")
                        {
                            if (from >= length)
                            {
                                return "";
                            }

                            if (from + count > length || count <= 0)
                            {
                                return name[from..];
                            }

                            return name.Substring(from, count);
                        }
                        else
                        {
                            int realFrom = length - from - count;
                            if (realFrom >= length)
                            {
                                return "";
                            }

                            if (from + count > length || count <= 0)
                            {
                                return name[..(length - from)];
                            }

                            return name.Substring(realFrom, count);
                        }
                    }
                    catch
                    {
                        return "";
                    }
                }
            },
            //文件大小
            { "<Size>", (item, arg) => item is SimpleFileInfo f ? f.Length.ToString() : "0" },
            //文件夹名
            { "<Directory>", (item, arg) => Path.GetDirectoryName(item.Path) },
            //创建时间
            {
                "<CreatTime-" + DateTimeRegexString + ">",
                (item, arg) =>
                    File.GetCreationTime(item.Path).ToString(Regex.Match(arg, "<CreatTime-" + DateTimeRegexString + ">")
                        .Groups["arg"]
                        .Value)
            },
            //创建时间UTC
            {
                "<CreatTimeUtc-" + DateTimeRegexString + ">",
                (item, arg) =>
                    File.GetCreationTimeUtc(item.Path).ToString(Regex
                        .Match(arg, "<CreatTimeUtc-" + DateTimeRegexString + ">")
                        .Groups["arg"].Value)
            },
            //上次访问时间
            {
                "<LastAccessTime-" + DateTimeRegexString + ">",
                (item, arg) =>
                    File.GetLastAccessTime(item.Path).ToString(Regex
                        .Match(arg, "<LastAccessTime-" + DateTimeRegexString + ">")
                        .Groups["arg"].Value)
            },
            {
                "<LastAccessTimeUtc-" + DateTimeRegexString + ">",
                (item, arg) =>
                    File.GetLastAccessTimeUtc(item.Path).ToString(Regex
                        .Match(arg, "<LastAccessTimeUtc-" + DateTimeRegexString + ">")
                        .Groups["arg"].Value)
            },
            //修改时间
            {
                "<LastWriteTime-" + DateTimeRegexString + ">",
                (item, arg) =>
                    item.Time.ToString(Regex.Match(arg, "<LastWriteTime-" + DateTimeRegexString + ">")
                        .Groups["arg"].Value)
            },
            {
                "<LastWriteTimeUtc-" + DateTimeRegexString + ">",
                (item, arg) =>
                    item.Time.ToString(Regex.Match(arg, "<LastWriteTimeUtc-" + DateTimeRegexString + ">")
                        .Groups["arg"].Value)
            },
            //Exif信息
            // {
            //     "{Exif-(?<attri>[a-zA-Z]+)}", (item, arg) =>
            //     {
            //         try
            //         {
            //             string attri = Regex.Match(arg, "{Exif-(?<attri>[a-zA-Z]+)}").Groups["attri"].Value;
            //             var exfi = new EXIFMetaData();
            //             EXIFMetaData.Metadata m = exfi.GetEXIFMetaData(item.FullName);
            //             var fields = m.GetType().GetFields();
            //             if (!fields.Any(p => p.Name == attri))
            //             {
            //                 return "";
            //             }
            //
            //             var result = fields.First(p => p.Name == attri).GetValue(m) as EXIFMetaData.MetadataDetail?;
            //             if (!result.HasValue)
            //             {
            //                 return "";
            //             }
            //             else
            //             {
            //                 return result.Value.DisplayValue;
            //             }
            //         }
            //         catch
            //         {
            //             return "";
            //         }
            //     }
            // }
        };

    private const string DateTimeRegexString = @"(?<arg>[a-zA-Z\-]+)";
    private const string SubStringRegexString = @"(?<Direction>Left|Right)-(?<From>[0-9]+)-(?<Count>[0-9]+)";
    private static readonly Dictionary<string, Regex> regexes = new Dictionary<string, Regex>();
    private string[] replacePatterns;
    public override RenameConfig Config { get; } = config;
    public IReadOnlyList<RenameFileInfo> Files { get; private set; }

    public override async Task ExecuteAsync(CancellationToken token = default)
    {
        var processingFiles = Files.Where(p => p.IsMatched && p.IsChecked).ToList();
        var duplicates = processingFiles
            .Select(p => p.NewPath)
            .GroupBy(p => p)
            .Where(p => p.Count() > 1)
            .Select(p => p.Key);
        if (duplicates.Any())
        {
            throw new Exception("有一些文件（夹）的目标路径相同：" + string.Join('、', duplicates));
        }

        //重命名为临时文件名，避免有可能新的文件名和其他文件的旧文件名一致导致错误的问题
        await TryForFilesAsync(processingFiles, (file, s) =>
        {
            NotifyMessage($"正在重命名（第一步，共二步）{s.GetFileNumberMessage()}：{file.Name}=>{file.NewName}");
            file.TempPath = Path.Combine(Path.GetDirectoryName(file.Path), Guid.NewGuid().ToString());
            File.Move(file.Path, file.TempPath);
        }, token, FilesLoopOptions.Builder().AutoApplyFileNumberProgress().Build());

        //重命名为目标文件名
        await TryForFilesAsync(processingFiles, (file, s) =>
        {
            NotifyMessage($"正在重命名（第一步，共二步）{s.GetFileNumberMessage()}：{file.Name}=>{file.NewName}");
            File.Move(file.TempPath, file.NewPath);
        }, token, FilesLoopOptions.Builder().AutoApplyStatus().AutoApplyFileNumberProgress().Build());
    }

    public override async Task InitializeAsync(CancellationToken token = default)
    {
        regexOptions = Config.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
        stringComparison = Config.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        NotifyProgressIndeterminate();
        NotifyMessage("正在查找文件");
        await Task.Run(() =>
        {
            PreprocessReplacePattern();
            IEnumerable<FileSystemInfo> files;
            if (Config.RenameTarget == RenameTargetType.File)
            {
                files = new DirectoryInfo(Config.Dir)
                    .EnumerateFiles("*", new EnumerationOptions()
                    {
                        IgnoreInaccessible = true,
                        RecurseSubdirectories = true,
                    });
            }
            else
            {
                files = new DirectoryInfo(Config.Dir)
                    .EnumerateDirectories("*", new EnumerationOptions()
                    {
                        IgnoreInaccessible = true,
                        RecurseSubdirectories = true,
                    });
            }

            List<RenameFileInfo> renameFiles = new List<RenameFileInfo>();

            TryForFiles(files.Select(file => new RenameFileInfo(file)), (renameFile, s) =>
            {
                NotifyMessage($"正在处理文件{s.GetFileNumberMessage()}：{renameFile.Name}");
                renameFiles.Add(renameFile);
                renameFile.IsMatched = IsMatched(renameFile);
                if (renameFile.IsMatched)
                {
                    renameFile.NewName = Rename(renameFile);
                    renameFile.NewPath = Path.Combine(Path.GetDirectoryName(renameFile.Path), renameFile.NewName);
                }
            }, token, FilesLoopOptions.DoNothing());

            Files = renameFiles.AsReadOnly();
        }, token);
    }

    private Regex GetRegex(string pattern)
    {
        if (regexes.TryGetValue(pattern, out Regex r))
        {
            return r;
        }

        r = new Regex(pattern, regexOptions);
        regexes.Add(pattern, r);
        return r;
    }

    private RegexOptions regexOptions;

    private string GetTargetName(RenameFileInfo file)
    {
        return string.Concat(replacePatterns.Select(p =>
        {
            if (p[0] == '<' && fileAttributesDic.TryGetValue(p, out Func<SimpleFileInfo, string, string> func))
            {
                return func(file, p);
            }

            return p;
        }));
    }

    private bool IsMatched(RenameFileInfo file)
    {
        string name = Config.SearchPath ? file.Path : file.Name;

        return Config.SearchMode switch
        {
            SearchMode.Contain => name.Contains(Config.SearchPattern, stringComparison),
            SearchMode.EqualWithExtension => Path.GetExtension(name).Equals(Config.SearchPattern, stringComparison),
            SearchMode.EqualWithName => Path.GetFileNameWithoutExtension(name)
                .Equals(Config.SearchPattern, stringComparison),
            SearchMode.Equal => name.Equals(Config.SearchPattern, stringComparison),
            SearchMode.Regex => GetRegex(Config.SearchPattern).IsMatch(name),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void PreprocessReplacePattern()
    {
        string text = Config.ReplacePattern;
        if (text == null)
        {
            if (Config.RenameMode is RenameMode.RetainMatched or RenameMode.RetainMatchedExtension)
            {
                return;
            }
            else
            {
                text = "";
            }
        }
        List<string> list = new List<string>();
        int left = 0;
        int right = 0;
        bool hasLeftBracket = false;
        while (right < text.Length)
        {
            char c = text[right];
            switch (c)
            {
                case '<':
                    hasLeftBracket = true;
                    if (right > left)
                    {
                        list.Add(text[left..right]);
                        left = right;
                    }

                    right++;
                    break;
                case '>':
                    right++;
                    if (hasLeftBracket)
                    {
                        hasLeftBracket = false;
                        list.Add(text[left..right]);
                        left = right;
                    }

                    break;
                default:
                    right++;
                    break;
            }
        }

        if (left != right)
        {
            list.Add(text[left..]);
        }

        replacePatterns = [.. list];
    }

    private StringComparison stringComparison;

    private string Rename(RenameFileInfo file)
    {
        string name = file.Name;
        string matched = null;
        if (Config.RenameMode is RenameMode.ReplaceMatched or RenameMode.RetainMatched or RenameMode.RetainMatchedExtension)
        {
            matched = Config.SearchMode == SearchMode.Regex
                ? GetRegex(Config.SearchPattern).Match(name).Value
                : Config.SearchPattern;
        }

        return Config.RenameMode switch
        {
            RenameMode.ReplaceMatched => name.Replace(matched, GetTargetName(file), stringComparison),
            RenameMode.ReplaceExtension => $"{Path.GetFileNameWithoutExtension(name)}.{GetTargetName(file)}",
            RenameMode.ReplaceName => GetTargetName(file) + Path.GetExtension(name),
            RenameMode.ReplaceAll => GetTargetName(file),
            RenameMode.RetainMatched => matched,
            RenameMode.RetainMatchedExtension => $"{matched}{Path.GetExtension(name)}",
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}