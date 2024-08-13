using FzLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArchiveMaster.Configs;
using ArchiveMaster.Enums;
using ArchiveMaster.Utilities;
using ArchiveMaster.ViewModels;

namespace ArchiveMaster.Utilities
{
    public class DirStructureSyncUtility(DirStructureSyncConfig config) : TwoStepUtilityBase
    {
        public override DirStructureSyncConfig Config { get; } = config;
        public IList<MatchingFileInfo> ExecutingFiles { get; set; }
        private readonly Dictionary<long, object> length2Template = new Dictionary<long, object>();
        private readonly Dictionary<DateTime, object> modifiedTime2Template = new Dictionary<DateTime, object>();
        private readonly Dictionary<string, object> name2Template = new Dictionary<string, object>();
        public List<MatchingFileInfo> RightPositionFiles { get; private set; }
        public List<MatchingFileInfo> WrongPositionFiles { get; private set; }

        public override async Task InitializeAsync(CancellationToken token)
        {
          List<MatchingFileInfo> rightPositionFiles = new List<MatchingFileInfo>();
            List<MatchingFileInfo> wrongPositionFiles = new List<MatchingFileInfo>();

            await Task.Run(() =>
            {
                var blacks = new BlackListUtility(Config.BlackList, Config.BlackListUseRegex);

                List<FileInfo> notMatchedFiles = new List<FileInfo>();
                List<FileInfo> matchedFiles = new List<FileInfo>();

                List<MatchingFileInfo> tempFiles = new List<MatchingFileInfo>();


                //分析模板目录，创建由属性指向文件的字典
                CreateDictionaries(Config.TemplateDir, Config.MaxTimeToleranceSecond, token);

                NotifyMessage($"正在查找源文件");

                //枚举源目录
                var sourceFiles = new DirectoryInfo(Config.SourceDir)
                    .EnumerateFiles("*", new EnumerationOptions()
                    {
                        IgnoreInaccessible = true,
                        AttributesToSkip = 0,
                        RecurseSubdirectories = true,
                    }).ToList();

                int index = 0;
                //对每个源文件进行匹配分析
                foreach (var sourceFile in sourceFiles)
                {
                    token.ThrowIfCancellationRequested();
                    index++;
                    //黑名单检测
                    if (blacks.IsInBlackList(sourceFile))
                    {
                        continue;
                    }

                    NotifyProgress(1.0*index/sourceFiles.Count);
                    NotifyMessage($"正在分析源文件（{index}/{sourceFiles.Count}）：{sourceFile.FullName}");
                    matchedFiles.Clear();
                    tempFiles.Clear();

                    //与模板文件进行匹配
                    bool notMatched = false;
                    if (Config.CompareName) GetMatchedFiles(name2Template, sourceFile.Name);
                    if (!notMatched && Config.CompareTime)
                        GetMatchedFiles(modifiedTime2Template, TruncateToSecond(sourceFile.LastWriteTime));
                    if (!notMatched && Config.CompareLength) GetMatchedFiles(length2Template, sourceFile.Length);

                    if (notMatched) //无匹配，不需要继续操作
                    {
                        notMatchedFiles.Add(sourceFile);
                        continue;
                    }

                    //对与该源文件匹配的模板文件（一般来说就一个）进行处理
                    foreach (var templateFile in matchedFiles)
                    {
                        //创建模板文件数据结构
                        var template = new SimpleFileInfo() //模板文件
                        {
                            Path = Path.GetRelativePath(Config.TemplateDir, templateFile.FullName),
                            Name = templateFile.Name,
                            Length = templateFile.Length,
                            Time = templateFile.LastWriteTime,
                        };

                        //创建模型
                        var goHomeFile = new MatchingFileInfo() //源文件
                        {
                            Path = Path.GetRelativePath(Config.SourceDir, sourceFile.FullName),
                            Name = sourceFile.Name,
                            Length = sourceFile.Length,
                            Time = sourceFile.LastWriteTime,
                            MultipleMatches = matchedFiles.Count > 1,
                            Template = template,
                        };

                        goHomeFile.RightPosition = template.Path == goHomeFile.Path;
                        tempFiles.Add(goHomeFile);
                    }

                    if (tempFiles.Count == 1) //如果只有一个匹配的文件
                    {
                        if (tempFiles[0].RightPosition) //位置正确
                        {
                            rightPositionFiles.Add(tempFiles[0]);
                        }
                        else //位置错误
                        {
                            tempFiles[0].IsChecked = true;
                            wrongPositionFiles.Add(tempFiles[0]);
                        }
                    }
                    else //有多个匹配
                    {
                        if (tempFiles.Any(p => p.RightPosition))
                        {
                            var tempWrongPositionFiles = tempFiles.Where(p => !p.RightPosition);
                            tempWrongPositionFiles.ForEach(p => p.Warn("包含一个位置正确的匹配"));
                            wrongPositionFiles.AddRange(tempFiles.Where(p => !p.RightPosition));
                            rightPositionFiles.Add(tempFiles.First(p => p.RightPosition));
                        }
                        else
                        {
                            wrongPositionFiles.AddRange(tempFiles);
                        }
                    }


                    //在指定字典中查找符合所给的属性值的文件
                    void GetMatchedFiles<TK>(Dictionary<TK, object> dic, TK key)
                    {
                        object files = null;
                        if (!dic.TryGetValue(key, out var value))
                        {
                            notMatched = true;
                            return;
                        }

                        files = value;
                        //}

                        if (matchedFiles.Count == 0) //如果notMatched==false，但matchedFiles.Count==0，说明这是第一个匹配
                        {
                            if (files is FileInfo f)
                            {
                                matchedFiles.Add(f);
                            }
                            else if (files is List<FileInfo> list)
                            {
                                matchedFiles.AddRange(list);
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else //如果不是第一个匹配，已经有规则匹配了
                        {
                            if (files is FileInfo f)
                            {
                                if (!matchedFiles.Contains(f)) //单个文件，但是当前匹配不在之前匹配的文件中，说明匹配失败
                                {
                                    matchedFiles.Clear();
                                }
                                else //否则，修改为本次匹配的单个文件
                                {
                                    matchedFiles.Clear();
                                    matchedFiles.Add(f);
                                }
                            }
                            else if (files is List<FileInfo> list)
                            {
                                var oldMatchedFiles = new List<FileInfo>(matchedFiles);
                                matchedFiles.Clear();
                                matchedFiles.AddRange(oldMatchedFiles.Intersect(list));
                            }
                            else
                            {
                                throw new Exception();
                            }

                            if (matchedFiles.Count == 0) //如果经过这一轮的匹配，没有任何文件完成匹配了，说明总的匹配失败
                            {
                                notMatched = true;
                            }
                        }
                    }
                }
            }, token);
            RightPositionFiles = rightPositionFiles;
            WrongPositionFiles = wrongPositionFiles;
        }

        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);

        private DateTime TruncateToSecond(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
            {
                return dateTime;
            }

            return dateTime.AddTicks(-(dateTime.Ticks % OneSecond.Ticks));
        }


        private void CreateDictionaries(string dir, int maxTimeTolerance, CancellationToken token)
        {
            name2Template.Clear();
            length2Template.Clear();
            modifiedTime2Template.Clear();
            var fileInfos = new DirectoryInfo(dir)
                .EnumerateFiles("*", new EnumerationOptions()
                {
                    IgnoreInaccessible = true,
                    AttributesToSkip = 0,
                    RecurseSubdirectories = true,
                });

            foreach (var file in fileInfos)
            {
                NotifyProgressIndeterminate();
                NotifyMessage($"正在分析模板文件：{file.FullName}");
                SetOrAdd(name2Template, file.Name);
                SetOrAdd(length2Template, file.Length);
                for (int i = -maxTimeTolerance; i <= maxTimeTolerance; i++)
                {
                    SetOrAdd(modifiedTime2Template, TruncateToSecond(file.LastWriteTime).AddSeconds(i));
                }

                token.ThrowIfCancellationRequested();

                void SetOrAdd<TK>(Dictionary<TK, object> dic, TK key)
                {
                    if (dic.ContainsKey(key))
                    {
                        if (dic[key] is List<FileInfo> list)
                        {
                            list.Add(file);
                        }
                        else if (dic[key] is FileInfo f)
                        {
                            dic[key] = new List<FileInfo>() { f, file };
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else
                    {
                        dic.Add(key, file);
                    }
                }
            }
        }


        public override Task ExecuteAsync(CancellationToken token)
        {
            if (ExecutingFiles == null)
            {
                throw new NullReferenceException($"{nameof(ExecutingFiles)}为空");
            }

            string copyMoveText = Config.Copy ? "复制" : "移动";
            IEnumerable<MatchingFileInfo> files = ExecutingFiles;
            long count = files.Sum(p => p.Length);
            long progress = 0;
            if (!Config.Copy)
            {
                files = files.Where(p => !p.RightPosition);
            }

            return TryForFilesAsync(files, (file, s) =>
            {
                progress += file.Length;
                NotifyMessage($"正在{copyMoveText}{s.GetFileIndexAndCountMessage()}：{file.Path}");
                string destFile = Path.Combine(Config.TargetDir, file.Template.Path);
                string destFileDir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(destFileDir))
                {
                    Directory.CreateDirectory(destFileDir);
                }

                if (Config.Copy)
                {
                    File.Copy(Path.Combine(Config.SourceDir, file.Path), destFile);
                }
                else
                {
                    File.Move(Path.Combine(Config.SourceDir, file.Path), destFile);
                }

            }, token);
           
        }
    }
}