using Avalonia.Controls;

namespace AdRasta2.Extensions;

public static class NumericUpDownExtensions
{
    public static void PreventNull(this NumericUpDown control)
    {
        control.ValueChanged += (s, e) =>
        {
            if (e.NewValue == null)
                control.Value = 0;
        };
    }
}
