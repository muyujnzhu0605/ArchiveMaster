using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace ArchiveMaster.Converters;

public class NotNullGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string str)
        {
            throw new ArgumentException("参数必须为字符串", nameof(parameter));
        }

        var length = value == null ? new GridLength(0) : GridLength.Parse(str);
        return length;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string str)
        {
            throw new ArgumentException("参数必须为字符串", nameof(parameter));
        }

        var length = value == null ? GridLength.Parse(str) : new GridLength(0);
        return length;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}