using System.Text.RegularExpressions;

namespace ArchiveMaster.Utilities;

public static class BlackListUtility
{
    public static void InitializeBlackList(string blackList, bool blackListUseRegex, out string[] blackStrings,
        out Regex[] blackRegexs)
    {
        blackStrings = blackList?.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries) ??
                       Array.Empty<string>();
        blackRegexs = null;
        if (blackListUseRegex)
        {
            try
            {
                blackRegexs = blackStrings.Select(p => new Regex(p, RegexOptions.IgnoreCase)).ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("黑名单正则解析失败", ex);
            }
        }
    }

    /// <summary>
    /// 文件是否在黑名单中
    /// </summary>
    /// <param name="name">文件名</param>
    /// <param name="path">文件路径</param>
    /// <param name="blackList">黑名单列表</param>
    /// <param name="blackRegexs">黑名单正则列表</param>
    /// <param name="blackListUseRegex">是否启用正则</param>
    /// <returns></returns>
    public static bool IsInBlackList(string name, string path, IList<string> blackList, IList<Regex> blackRegexs,
        bool blackListUseRegex)
    {
        for (int i = 0; i < blackList.Count; i++)
        {
            if (blackListUseRegex) //正则
            {
                if (blackList[i].Contains('\\') || blackList[i].Contains('/')) //目录
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
                if (blackList[i].Contains('\\') || blackList[i].Contains('/')) //目录
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