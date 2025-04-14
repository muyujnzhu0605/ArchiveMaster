using System.Globalization;
using ArchiveMaster.ViewModels.FileSystem;
using Avalonia.Data.Converters;
using FzLib;

namespace ArchiveMaster.Converters;

public class FileDirLength2StringConverter : IValueConverter
{
    public string DirString { get; set; } = "文件夹";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return null;
        }

        var fileOrDir = value as SimpleFileInfo ?? throw new Exception("值必须为SimpleFileDirInfo类型");
        if (fileOrDir.IsDir)
        {
            return DirString;
        }

        return NumberConverter.ByteToFitString(fileOrDir.Length, 2, " B", " KB", " MB", " GB", " TB");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}