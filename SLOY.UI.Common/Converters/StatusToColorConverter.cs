using System.Globalization;
using SLOY.Shared.Enums;

namespace SLOY.UI.Common.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NodeStatus status)
        {
            return status switch
            {
                NodeStatus.Online => Color.FromArgb("#00FFAA"),
                NodeStatus.Away => Color.FromArgb("#FFAA00"),
                NodeStatus.DoNotDisturb => Color.FromArgb("#FF4444"),
                NodeStatus.Emergency => Color.FromArgb("#FF0000"),
                NodeStatus.Offline => Color.FromArgb("#555555"),
                _ => Color.FromArgb("#555555")
            };
        }
        return Color.FromArgb("#555555");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}