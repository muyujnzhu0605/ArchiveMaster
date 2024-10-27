using System.Globalization;
using Avalonia.Data.Converters;

namespace ArchiveMaster.Converters;

public class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (value)
        {
            case null:
            case DateTime { Ticks: 0 }:
                return "";
            case DateTime dt:
                return dt.ToString(CultureInfo.CurrentCulture.DateTimeFormat);
            default:
                throw new ArgumentException("值不为DateTime类型", nameof(value));
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                if (targetType == typeof(DateTime))
                {
                    return default(DateTime);
                }

                if (targetType == typeof(DateTime?))
                {
                    return null;
                }

                throw new ArgumentException("不支持的类型", nameof(targetType));
            }

            return DateTime.Parse(str);
        }

        throw new ArgumentException("非String欲转DateTime", nameof(value));
    }
}