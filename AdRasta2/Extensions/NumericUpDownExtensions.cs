using Avalonia.Controls;

namespace AdRasta2.Extensions;

public static class NumericUpDownExtensions
{
    public static void PreventNull(this NumericUpDown control)
    {
        control.ValueChanged += (_, e) =>
        {
            if (e.NewValue == null)
            {
                if (control.Tag is string tagString)
                {
                    if (int.TryParse(tagString, out var intFallback))
                        control.Value = intFallback;
                    else if (decimal.TryParse(tagString, out var decimalFallback))
                        control.Value = (decimal)decimalFallback;
                    else
                        control.Value = control.Minimum;
                }
                else if (control.Tag is int intTag)
                    control.Value = intTag;
                else if (control.Tag is decimal decimalTag)
                    control.Value = (decimal)decimalTag;
                else
                    control.Value = control.Minimum;
            }
        };
    }
}


