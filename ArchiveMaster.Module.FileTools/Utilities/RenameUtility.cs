// using ArchiveMaster.Configs;
//
// namespace ArchiveMaster.Utilities;
//
// public class RenameUtility(RenameConfig config) : TwoStepUtilityBase
// {
//     public override RenameConfig Config { get; } = config;
//
//     public override Task ExecuteAsync(CancellationToken token = default)
//     {
//         throw new NotImplementedException();
//     }
//
//     public override Task InitializeAsync(CancellationToken token = default)
//     {
//         throw new NotImplementedException();
//     }
//
//         /// <summary>
//         /// 列举所有的文件（夹）
//         /// </summary>
//         /// <returns></returns>
//         public async static Task<bool> ListFiles(IList<RenameFileInfo> listItems, bool isFolder, bool includeSubfolders)
//         {
//             if (Path == "")
//             {
//                 return false;
//             }
//             List<string> notExistPath = new List<string>();
//             Dictionary<string, string> errorPath = new Dictionary<string, string>();
//             listItems.Clear();
//             var paths = Paths;
//             await Task.Run(() =>
//             {
//                 foreach (var path in paths)
//                 {
//                     if (FileSystem.FileExistsCaseSensitive(path))
//                     {
//                         App.Current.Dispatcher.Invoke(() => listItems.Add(new RenameFileInfo(new FileInfo(path))));
//                     }
//                     else if (Directory.Exists(path))
//                     {
//                         try
//                         {
//                             if (isFolder)
//                             {
//                                 GetFolders(path);
//                             }
//                             else
//                             {
//                                 GetFiles(path);
//                             }
//                         }
//                         catch (Exception ex)
//                         {
//                             errorPath.Add(path, ex.Message);
//                         }
//                     }
//                     else
//                     {
//                         notExistPath.Add(path);
//                     }
//                 }
//             });
//
//             if (listItems.Count == 0)
//             {
//                 return false;
//             }
//             if (notExistPath.Count != 0 || errorPath.Count != 0)
//             {
//                 StringBuilder str = new StringBuilder();
//
//                 if (notExistPath.Count != 0)
//                 {
//                     str.AppendLine("以下地址无法被识别：");
//                     str.AppendLine();
//                     foreach (var item in notExistPath)
//                     {
//                         str.Append(item);
//                     }
//                 }
//                 if (errorPath.Count != 0)
//                 {
//                     if (notExistPath.Count != 0)
//                     {
//                         str.AppendLine();
//                         str.AppendLine();
//                     }
//                     {
//                         str.AppendLine("以下地址访问失败：");
//                         str.AppendLine();
//                         foreach (var item in errorPath)
//                         {
//                             str.Append(item.Key + "，原因：" + item.Value);
//                         }
//                     }
//                     ShowWarn(str.ToString());
//                 }
//             }
//             return true;
//
//             void GetFiles(string path)
//             {
//                 if (!includeSubfolders)
//                 {
//                     foreach (var i in Directory.EnumerateFiles(path))
//                     {
//                         App.Current.Dispatcher.Invoke(() => listItems.Add(new RenameFileInfo(new FileInfo(i))));
//                     }
//                 }
//                 else
//                 {
//                     foreach (var i in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
//                     {
//                         App.Current.Dispatcher.Invoke(() => listItems.Add(new RenameFileInfo(new FileInfo(i))));
//                     }
//                 }
//             }
//             void GetFolders(string path)
//             {
//                 List<RenameFileInfo> tempList = new List<RenameFileInfo>();
//                 if (!includeSubfolders)
//                 {
//                     foreach (var i in Directory.EnumerateDirectories(path))
//                     {
//                         tempList.Add(new RenameFileInfo(new DirectoryInfo(i)));
//                     }
//                 }
//                 else
//                 {
//                     foreach (var i in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
//                     {
//                         tempList.Add(new RenameFileInfo(new DirectoryInfo(i)));
//                     }
//                 }
//                 for (int i = tempList.Count - 1; i >= 0; i--)
//                 {
//                     App.Current.Dispatcher.Invoke(() => listItems.Add(tempList[i]));
//                 }
//             }
//         }
//
//         public static bool ListFiles(IList<MoveOrCopyFileInfo> listItems, bool isFolder, bool includeSubfolders)
//         {
//             if (Path == "")
//             {
//                 return false;
//             }
//             List<string> notExistPath = new List<string>();
//             Dictionary<string, string> errorPath = new Dictionary<string, string>();
//             listItems.Clear();
//             foreach (var path in Paths)
//             {
//                 if (FileSystem.FileExistsCaseSensitive(path))
//                 {
//                     listItems.Add(new MoveOrCopyFileInfo(new FileInfo(path), path));
//                 }
//                 else if (Directory.Exists(path))
//                 {
//                     try
//                     {
//                         GetFiles(path, path);
//                     }
//                     catch (Exception ex)
//                     {
//                         errorPath.Add(path, ex.Message);
//                     }
//                 }
//                 else
//                 {
//                     notExistPath.Add(path);
//                 }
//             }
//
//             if (listItems.Count == 0)
//             {
//                 return false;
//             }
//             if (notExistPath.Count != 0 || errorPath.Count != 0)
//             {
//                 StringBuilder str = new StringBuilder();
//
//                 if (notExistPath.Count != 0)
//                 {
//                     str.AppendLine("以下地址无法被识别：");
//                     str.AppendLine();
//                     foreach (var item in notExistPath)
//                     {
//                         str.Append(item);
//                     }
//                 }
//                 if (errorPath.Count != 0)
//                 {
//                     if (notExistPath.Count != 0)
//                     {
//                         str.AppendLine();
//                         str.AppendLine();
//                     }
//                     {
//                         str.AppendLine("以下地址访问失败：");
//                         str.AppendLine();
//                         foreach (var item in errorPath)
//                         {
//                             str.Append(item.Key + "，原因：" + item.Value);
//                         }
//                     }
//                     ShowWarn(str.ToString());
//                 }
//             }
//             return true;
//
//             void GetFiles(string path, string root)
//             {
//                 if (!includeSubfolders)
//                 {
//                     foreach (var i in Directory.EnumerateFiles(path))
//                     {
//                         listItems.Add(new MoveOrCopyFileInfo(new FileInfo(i), root));
//                     }
//                 }
//                 else
//                 {
//                     foreach (var i in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
//                     {
//                         listItems.Add(new MoveOrCopyFileInfo(new FileInfo(i), root));
//                     }
//                 }
//             }
//         }
//
//         public static void TryStop()
//         {
//             stop = true;
//         }
//
//         private static bool stop = false;
//
//         public async static Task Rename(IList<RenameFileInfo> listItems, bool isFolder, int sameNameMode, bool recordHistory = true)
//         {
//             win.taskBar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
//             var history = new RenameHistoryHelper(isFolder);
//             await Task.Run(() =>
//              {
//                  int count = listItems.Count;
//                  int index = 0;
//                  foreach (var item in listItems)
//                  {
//                      if (stop)
//                      {
//                          stop = false;
//                          break;
//                      }
//                      App.Current.Dispatcher.Invoke(() =>
//                      {
//                          win.Title = "文件批量操作 - 正在替换（" + (++index).ToString() + "/" + count + "）";
//                          win.taskBar.ProgressValue = 1.0 * index / count;
//                      });
//
//                      if (!item.Ready)
//                      {
//                          continue;
//                      }
//                      if (!isFolder)
//                      {
//                          string newPath = Combine(item.File.DirectoryName, item.NewName);
//
//                          if (FileSystem.FileExistsCaseSensitive(newPath))
//                          {
//                              NotExistNameResult result = NotExistNameResult.None; string tryNewPath = "";
//                              App.Current.Dispatcher.Invoke(() =>
//                              {
//                                  result = GetNotExistFileName(item.OldName, Combine(item.File.Directory.FullName, item.NewName), sameNameMode, Brushes.LightGray, out tryNewPath);
//                              });
//                              if (result == NotExistNameResult.Abort)
//                              {
//                                  return;
//                              }
//                              else if (result == NotExistNameResult.Ignore)
//                              {
//                                  item.Status = "忽略";
//                                  continue;
//                              }
//
//                              newPath = tryNewPath;
//                              item.NewName = new FileInfo(newPath).Name;
//                          }
//                          if (newPath != "")
//                          {
//                              try
//                              {
//                                  string oldPath = item.File.FullName;
//                                  item.File.MoveTo(newPath);
//                                  item.Status = "成功";
//                                  history.Add(oldPath, newPath);
//                              }
//                              catch (System.Security.SecurityException)
//                              {
//                                  item.Status = "错误：权限不足";
//                              }
//                              catch (UnauthorizedAccessException)
//                              {
//                                  item.Status = "错误：目录为只读";
//                              }
//                              catch (FileNotFoundException)
//                              {
//                                  item.Status = "错误：源文件不存在";
//                              }
//                              catch (DirectoryNotFoundException)
//                              {
//                                  item.Status = "错误：目录不存在";
//                              }
//                              catch (PathTooLongException)
//                              {
//                                  item.Status = "错误：文件名过长";
//                              }
//                              catch (IOException)
//                              {
//                                  item.Status = "错误：其他IO";
//                              }
//                              catch (Exception ex)
//                              {
//                                  item.Status = "错误：" + ex.Message;
//                              }
//                          }
//                          else
//                          {
//                              item.Status = "忽略";
//                          }
//                      }
//                      else
//                      {
//                          if (item.NewName == "")
//                          {
//                              item.Status = "目标文件夹名为空";
//                          }
//                          // string newPath = item.Directory.FullName.Replace(item.OldName, item.NewName);
//                          string newPath = Combine(item.Directory.Parent.FullName, item.NewName);
//                          if (Directory.Exists(newPath))
//                          {
//                              var result = GetNotExistDirectoryName(item.OldName, Combine(item.Directory.Parent.FullName, item.NewName), sameNameMode, Brushes.LightGray, out string tryNewPath);
//                              if (result == NotExistNameResult.Abort)
//                              {
//                                  return;
//                              }
//                              else if (result == NotExistNameResult.Ignore)
//                              {
//                                  item.Status = "忽略";
//                                  continue;
//                              }
//
//                              newPath = tryNewPath;
//                              item.NewName = new DirectoryInfo(newPath).Name;
//                          }
//                          if (newPath != "")
//                          {
//                              try
//                              {
//                                  item.Directory.MoveTo(newPath);
//                                  item.Status = "成功";
//                              }
//                              catch (System.Security.SecurityException)
//                              {
//                                  item.Status = "错误：权限不足";
//                              }
//                              catch (UnauthorizedAccessException)
//                              {
//                                  item.Status = "错误：目录为只读";
//                              }
//                              catch (FileNotFoundException)
//                              {
//                                  item.Status = "错误：源文件不存在";
//                              }
//                              catch (DirectoryNotFoundException)
//                              {
//                                  item.Status = "错误：目录不存在";
//                              }
//                              catch (PathTooLongException)
//                              {
//                                  item.Status = "错误：文件名过长";
//                              }
//                              catch (IOException)
//                              {
//                                  item.Status = "错误：其他IO";
//                              }
//                              catch (Exception ex)
//                              {
//                                  item.Status = "错误：" + ex.Message;
//                              }
//                          }
//                          else
//                          {
//                              item.Status = "忽略";
//                          }
//                      }
//
//                      item.Ready = false;
//                  }
//              });
//             win.Title = "文件批量操作";
//             win.taskBar.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
//             if (recordHistory)
//             {
//                 history.Save();
//             }
//         }
//
//         public static NotExistNameResult GetNotExistFileName(string oldName, string tryPath, int sameNameMode, SolidColorBrush inputBoxColor, out string newPath)
//         {
//             newPath = tryPath;
//
//             switch (sameNameMode)
//             {
//                 case 0:
//                     return NotExistNameResult.Ignore;
//
//                 case 1:
//                     do
//                     {
//                         FileInfo file = new FileInfo(newPath);
//                         var box = new InputBox($"文件“{oldName}”的新名字“{file.Name}”已存在，请指定新的文件名：", win, inputBoxColor, file.Name, ".*");
//                         box.AddButton("终止");
//                         box.AddButton("忽略");
//                         box.AddButton("确定", true, true);
//                         box.ShowDialog();
//                         if (box.ResultIndex == 0)
//                         {
//                             return NotExistNameResult.Abort;
//                         }
//                         else if (box.ResultIndex == 1)
//                         {
//                             return NotExistNameResult.Ignore;
//                         }
//                         else
//                         {
//                             newPath = Combine(file.DirectoryName, box.ResultText);
//                         }
//                     } while (FileSystem.FileExistsCaseSensitive(newPath));
//                     return NotExistNameResult.Success;
//
//                 case 2:
//                     int index = 1;
//                     string tryName = new FileInfo(newPath).Name;
//                     do
//                     {
//                         FileInfo file = new FileInfo(newPath);
//                         if (file.Extension != "")
//                         {
//                             newPath = Combine(file.DirectoryName, tryName.Replace(file.Extension, "") + " (" + (++index).ToString() + ")" + file.Extension);
//                         }
//                         else
//                         {
//                             newPath = Combine(file.DirectoryName, tryName) + " (" + (++index).ToString() + ")";
//                         }
//                     } while (FileSystem.FileExistsCaseSensitive(newPath));
//                     return NotExistNameResult.Success;
//             }
//             return NotExistNameResult.Abort;
//         }
//
//         public static NotExistNameResult GetNotExistDirectoryName(string oldName, string tryPath, int sameNameMode, SolidColorBrush inputBoxColor, out string newPath)
//         {
//             newPath = tryPath;
//
//             switch (sameNameMode)
//             {
//                 case 0:
//                     return NotExistNameResult.Ignore;
//
//                 case 1:
//                     do
//                     {
//                         DirectoryInfo directory = new DirectoryInfo(newPath);
//                         var box = new InputBox($"文件“{oldName}”的新名字“{directory.Name}”已存在，请指定新的文件名：", win, inputBoxColor, directory.Name, ".*");
//                         box.AddButton("终止");
//                         box.AddButton("忽略");
//                         box.AddButton("确定", true, true);
//                         box.ShowDialog();
//                         if (box.ResultIndex == 0)
//                         {
//                             return NotExistNameResult.Abort;
//                         }
//                         else if (box.ResultIndex == 1)
//                         {
//                             return NotExistNameResult.Ignore;
//                         }
//                         else
//                         {
//                             newPath = Combine(directory.Parent.FullName, box.ResultText);
//                         }
//                     } while (Directory.Exists(newPath));
//                     return NotExistNameResult.Success;
//
//                 case 2:
//                     int index = 1;
//                     string tryName = new DirectoryInfo(newPath).Name;
//                     do
//                     {
//                         DirectoryInfo directory = new DirectoryInfo(newPath);
//
//                         newPath = Combine(directory.Parent.FullName, tryName + " (" + (++index).ToString() + ")");
//                     } while (Directory.Exists(newPath));
//                     return NotExistNameResult.Success;
//             }
//             return NotExistNameResult.Abort;
//         }
//
//         public static string fileSizeToString(long size)
//         {
//             double dSize = size;
//             if (dSize == -1)
//             {
//                 return "";
//             }
//             if (dSize < 1024)
//             {
//                 return dSize.ToString() + "B";
//             }
//             dSize /= 1024;
//             if (dSize < 1024)
//             {
//                 return dSize.ToString("N2") + "KB";
//             }
//             dSize /= 1024;
//             if (dSize < 1024)
//             {
//                 return dSize.ToString("N2") + "MB";
//             }
//             dSize /= 1024;
//             if (dSize < 1024)
//             {
//                 return dSize.ToString("N2") + "GB";
//             }
//             dSize /= 1024;
//             return dSize.ToString("N2") + "TB";
//         }
//
//         private const string dateTimeRegexArg = "(?<arg>[a-zA-Z\\-]+)";
//         private const string subStringRegexArg = "(?<Direction>Left|Right)-(?<From>[0-9]+)-(?<Count>[0-9]+)";
//
//         public static Dictionary<string, Func<FileInfo, string, string>> fileAttributeReplace = new Dictionary<string, Func<FileInfo, string, string>>()
//         {
//             //文件名
//             {"{Name}",(item,arg)=>item.Name },
//             //无扩展名的文件名
//             {"{NameWithoutExtension}",(item,arg)=>System.IO.Path.GetFileNameWithoutExtension(item.Name) },
//             //文件扩展名
//             { "{NameExtension}",(item,arg)=>{string extension=item.Extension; return extension==""?"":extension.Replace(".",""); } },
//             //无扩展名的文件名截取
//            {"{NameWithoutExtension-"+subStringRegexArg+"}",(item,arg)=>
//            {
//                try
//                {
//                string extension=item.Extension;
//                string name= extension==""? item.Name:item.Name.Replace(extension,"");
//                Match match=Regex.Match(arg,"{NameWithoutExtension-"+subStringRegexArg+"}");
//                string direction=match.Groups["Direction"].Value;
//                int from=int.Parse(match.Groups["From"].Value);
//                int count=int.Parse(match.Groups["Count"].Value);
//                int length=name.Length;
//                if(direction=="Left")
//                {
//                    if(from>=length)
//                    {
//                        return "";
//                    }
//                    if(from+count>length || count<=0)
//                    {
//                        return name.Substring(from);
//                    }
//                    return name.Substring(from,count);
//                }
//                else
//                {
//                    int realFrom=length-from-count;
//                    if (realFrom>=length)
//                    {
//                        return "";
//                    }
//                   if(from+count>length || count<=0)
//                    {
//                        return name.Substring(0,length-from);
//                    }
//                    return name.Substring(realFrom,count);
//                }
//                }
//                catch
//                {
//                    return "";
//                }
//            } },
//            //文件大小
//             {"{Size}",(item,arg)=>fileSizeToString(item.Length) },
//             //文件夹名
//             {"{Directory}", (item,arg)=>item.Directory.Name},
//             //文件夹名
//             {"{FullDirectory}", (item,arg)=>item.DirectoryName},
//             //创建时间
//             { "{CreatTime-"+dateTimeRegexArg+"}" ,(item,arg)=>item.CreationTime.ToString(Regex.Match(arg,"{CreatTime-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             //创建时间UTC
//             { "{CreatTimeUtc-"+dateTimeRegexArg+"}" ,(item,arg)=>item.CreationTimeUtc.ToString(Regex.Match(arg,"{CreatTimeUtc-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             //上次访问时间
//             { "{LastAccessTime-"+dateTimeRegexArg+"}" ,(item,arg)=>item.LastAccessTime.ToString(Regex.Match(arg,"{LastAccessTime-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             {"{LastAccessTimeUtc-"+dateTimeRegexArg+"}" ,(item,arg)=>item.LastAccessTimeUtc.ToString(Regex.Match(arg,"{LastAccessTimeUtc-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             //修改时间
//             { "{LastWriteTime-"+dateTimeRegexArg+"}" ,(item,arg)=>item.LastWriteTime.ToString(Regex.Match(arg,"{LastWriteTime-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             {"{LastWriteTimeUtc-"+dateTimeRegexArg+"}" ,(item,arg)=>item.LastWriteTimeUtc.ToString(Regex.Match(arg,"{LastWriteTimeUtc-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             //Exif信息
//             { "{Exif-(?<attri>[a-zA-Z]+)}" ,(item,arg)=>
//             {
//                 try
//                 {
//                     string attri = Regex.Match(arg, "{Exif-(?<attri>[a-zA-Z]+)}").Groups["attri"].Value;
//                     var exfi = new EXIFMetaData();
//                     EXIFMetaData.Metadata m = exfi.GetEXIFMetaData(item.FullName);
//                     var fields = m.GetType().GetFields();
//                     if (!fields.Any(p => p.Name == attri))
//                     {
//                         return "";
//                     }
//                     var result = fields.First(p => p.Name == attri).GetValue(m) as EXIFMetaData.MetadataDetail?;
//                     if (!result.HasValue)
//                     {
//                         return "";
//                     }
//                     else
//                     {
//                         return result.Value.DisplayValue;
//                     }
//                 }
//                 catch
//                 {
//                     return "";
//                 }
//         }
//     }
//         };
//
//         public static Dictionary<string, Func<DirectoryInfo, string, string>> folderAttributeReplace = new Dictionary<string, Func<DirectoryInfo, string, string>>()
//         {
//             //文件夹名
//             {"{Name}",(item,arg)=>item.Name },
//             //文件夹名截取
//            {"{Name-"+subStringRegexArg+"}",(item,arg)=>
//            {
//                try
//                {
//                string name=item.Name;
//                Match match=Regex.Match(arg,"{Name-"+subStringRegexArg+"}");
//                string direction=match.Groups["Direction"].Value;
//                int from=int.Parse(match.Groups["From"].Value);
//                int count=int.Parse(match.Groups["Count"].Value);
//                int length=name.Length;
//                if(direction=="Left")
//                {
//                    if(from>=length)
//                    {
//                        return "";
//                    }
//                    if(from+count>length || count<=0)
//                    {
//                        return name.Substring(from);
//                    }
//                    return name.Substring(from,count);
//                }
//                else
//                {
//                    int realFrom=length-from-count;
//                    if (realFrom>=length)
//                    {
//                        return "";
//                    }
//                   if(from+count>length || count<=0)
//                    {
//                        return name.Substring(0,length-from);
//                    }
//                    return name.Substring(realFrom,count);
//                }
//                }
//                catch
//                {
//                    return "";
//                }
//            } },
//            //上级文件夹名
//             {"{Parent}",(item,arg)=>item.Parent.Name },
//             {"{FullParent}",(item,arg)=>item.Parent.FullName },
//
//             {"{CreatTime-"+dateTimeRegexArg+"}" ,(item,arg)=>item.CreationTime.ToString(Regex.Match(arg,"{CreatTime-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             {"{CreatTimeUtc-"+dateTimeRegexArg+"}" ,(item,arg)=>item.CreationTimeUtc.ToString(Regex.Match(arg,"{CreatTimeUtc-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             {"{LastAccessTime-"+dateTimeRegexArg+"}" ,(item,arg)=>item.LastAccessTime.ToString(Regex.Match(arg,"{LastAccessTime-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             {"{LastAccessTimeUtc-"+dateTimeRegexArg+"}" ,(item,arg)=>item.LastAccessTimeUtc.ToString(Regex.Match(arg,"{LastAccessTimeUtc-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             {"{LastWriteTime-"+dateTimeRegexArg+"}" ,(item,arg)=>item.LastWriteTime.ToString(Regex.Match(arg,"{LastWriteTime-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//             {"{LastWriteTimeUtc-"+dateTimeRegexArg+"}" ,(item,arg)=>item.LastWriteTimeUtc.ToString(Regex.Match(arg,"{LastWriteTimeUtc-"+dateTimeRegexArg+"}").Groups["arg"].Value) },
//         };
// }