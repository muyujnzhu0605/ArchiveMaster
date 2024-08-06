using System.Text.RegularExpressions;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Utilities;

public class BlackListUtility
{
    private readonly string blackList;
    private readonly bool blackListUseRegex;
    private readonly string[] blackStrings;
    private readonly Regex[] blackRegexs;

    public BlackListUtility(string blackList, bool blackListUseRegex)
    {
        this.blackList = blackList;
        this.blackListUseRegex = blackListUseRegex;
        blackStrings = blackList?.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries) ?? [];
        blackRegexs = null;
        if (!blackListUseRegex)
        {
            return;
        }

        try
        {
            blackRegexs = blackStrings.Select(p => new Regex(p, RegexOptions.IgnoreCase)).ToArray();
        }
        catch (Exception ex)
        {
            throw new Exception("黑名单正则解析失败", ex);
        }
    }


    public bool IsNotInBlackList(FileInfo file)
    {
        return !IsInBlackList(file);
    }

    public bool IsNotInBlackList(SimpleFileInfo file)
    {
        return !IsInBlackList(file);
    }

    public bool IsInBlackList(FileInfo file)
    {
        return IsInBlackList(file.Name, file.FullName);
    }

    public bool IsInBlackList(SimpleFileInfo file)
    {
        return IsInBlackList(file.Name, file.Path);
    }

    public bool IsNotInBlackList(string path)
    {
        return !IsInBlackList(path);
    }

    public bool IsInBlackList(string path)
    {
        return IsInBlackList(Path.GetFileName(path), path);
    }

    public bool IsNotInBlackList(string name, string path)
    {
        return !IsInBlackList(name, path);
    }

    public bool IsInBlackList(string name, string path)
    {
        for (int i = 0; i < blackStrings.Length; i++)
        {
            if (blackListUseRegex) //正则
            {
                if (blackStrings[i].Contains('\\') || blackStrings[i].Contains('/')) //目录
                {
                    path = path.Replace("\\", "/");
                    if (blackRegexs[i].IsMatch(path))
                    {
                        return true;
                    }
                }
                else //文件
                {
                    if (blackRegexs[i].IsMatch(name))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (blackStrings[i].Contains('\\') || blackStrings[i].Contains('/')) //目录
                {
                    path = path.Replace("\\", "/");
                    if (path.Contains(blackList[i]))
                    {
                        return true;
                    }
                }
                else //文件
                {
                    if (name.Contains(blackList[i]))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}