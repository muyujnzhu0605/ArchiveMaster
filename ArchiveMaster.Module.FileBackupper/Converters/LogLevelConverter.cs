using System.Globalization;
using Avalonia.Data.Converters;
using Microsoft.Extensions.Logging;

namespace ArchiveMaster.Converters;

public class LogLevelConverter : IValueConverter
{
    public static string GetDescription(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Debug => "调试",
            LogLevel.Information => "信息",
            LogLevel.Warning => "警告",
            LogLevel.Error => "错误",
            LogLevel.Critical => "严重",
            LogLevel.None => "无",
            _ => "未知"
        };
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? null : GetDescription((LogLevel)value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}