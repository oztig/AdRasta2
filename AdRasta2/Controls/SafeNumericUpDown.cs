using AdRasta2.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AdRasta2.Controls;

public class SafeNumericUpDown : NumericUpDown
{
    public static readonly StyledProperty<decimal?> DefaultIfNullProperty =
        AvaloniaProperty.Register<SafeNumericUpDown, decimal?>(nameof(DefaultIfNull));
    
    public static readonly StyledProperty<IBrush> SpinnerBorderBrushProperty =
        AvaloniaProperty.Register<SafeNumericUpDown, IBrush>(nameof(SpinnerBorderBrush));

    public static readonly StyledProperty<Thickness> SpinnerBorderThicknessProperty =
        AvaloniaProperty.Register<SafeNumericUpDown, Thickness>(nameof(SpinnerBorderThickness));

    public IBrush SpinnerBorderBrush
    {
        get => GetValue(SpinnerBorderBrushProperty);
        set => SetValue(SpinnerBorderBrushProperty, value);
    }

    public Thickness SpinnerBorderThickness
    {
        get => GetValue(SpinnerBorderThicknessProperty);
        set => SetValue(SpinnerBorderThicknessProperty, value);
    }


    public decimal? DefaultIfNull
    {
        get => GetValue(DefaultIfNullProperty);
        set => SetValue(DefaultIfNullProperty, value);
    }

    public SafeNumericUpDown()
    {
        Classes.Add("NumericUpDown");
        this.IfNullUseDefault();
    }
}