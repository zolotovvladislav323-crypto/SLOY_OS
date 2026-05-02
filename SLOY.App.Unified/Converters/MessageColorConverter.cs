using System.Globalization;

namespace SLOY.App.Unified;

public class MessageColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isMine)
            return isMine ? Color.FromArgb("#1A3A2A") : Color.FromArgb("#1A1A2A");
        return Color.FromArgb("#1A1A2A");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}