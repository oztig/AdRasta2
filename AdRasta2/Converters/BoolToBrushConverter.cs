using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AdRasta2.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public IBrush TrueBrush { get; set; } = new SolidColorBrush(Color.Parse("#888888")); // Default
    public IBrush FalseBrush { get; set; } = new SolidColorBrush(Colors.Black); // Default

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? TrueBrush : FalseBrush;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}