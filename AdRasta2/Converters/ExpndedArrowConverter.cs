using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AdRasta2.Converters;

public class ExpandArrowConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool isExpanded && isExpanded) ? 180 : 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Not used, but returning a safe default to keep Rider happy
        // Not used, but returning a harmless default to satisfy the interface
        return AvaloniaProperty.UnsetValue;
    }
}
