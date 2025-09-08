using System;
using System.Globalization;
using AdRasta2.Enums;
using AdRasta2.Models;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AdRasta2.Converters;

public class StatusToColourConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ConversionStatus status && StatusColours.Map.TryGetValue(status, out var colour))
            return new SolidColorBrush(colour);

        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
