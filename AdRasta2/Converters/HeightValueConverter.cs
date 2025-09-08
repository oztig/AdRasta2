using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AdRasta2.Converters;

public class HeightValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
            return intValue == 241 ? "Auto" : intValue.ToString();

        return "0"; // or some sensible default
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            if (str.Equals("Auto", StringComparison.OrdinalIgnoreCase))
                return 241;

            if (int.TryParse(str, out var result))
                return result;
        }

        return 0; // fallback
    }
}