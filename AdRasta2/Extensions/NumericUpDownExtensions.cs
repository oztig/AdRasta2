using AdRasta2.Controls;
using Avalonia.Controls;

namespace AdRasta2.Extensions;

public static class SafeNumericUpDownExtensions
{
    public static void IfNullUseDefault(this SafeNumericUpDown control)
    {
        control.ValueChanged += (_, e) =>
        {
            if (e.NewValue == null)
            {
                control.Value = control.DefaultIfNull ?? control.Minimum;
            }
        };
    }

}



