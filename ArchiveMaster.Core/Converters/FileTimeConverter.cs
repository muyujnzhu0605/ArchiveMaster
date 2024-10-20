using System.Globalization;
using Avalonia.Data.Converters;

namespace ArchiveMaster.Converters;

public class FileTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return "";
        }
        if (value is DateTime dt)
        {
            if (dt.Ticks == 0)
            {
                return "";
            }

            return dt.ToString(CultureInfo.CurrentCulture.DateTimeFormat);
        }

        throw new ArgumentException("值不为DateTime类型", nameof(value));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}