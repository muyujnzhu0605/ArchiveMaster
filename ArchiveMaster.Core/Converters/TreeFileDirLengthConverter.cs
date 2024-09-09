using System.Globalization;
using ArchiveMaster.ViewModels;
using Avalonia;
using Avalonia.Data.Converters;
using FzLib;

namespace ArchiveMaster.Converters;

public class TreeFileDirLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }
        var fileOrDir = value as TreeFileDirInfo ?? throw new Exception("值必须为TreeFileDirInfo类型");
        if (fileOrDir.IsDir && fileOrDir is TreeDirInfo dir)
        {
            return $"{dir.SubFolderCount}个子文件夹，{dir.SubFileCount}个子文件";
        }

        return NumberConverter.ByteToFitString(fileOrDir.Length);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}