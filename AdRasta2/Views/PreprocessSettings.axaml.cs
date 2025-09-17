using AdRasta2.Extensions;
using AdRasta2.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdRasta2.Views;

public partial class PreprocessSettings : UserControl
{
    public PreprocessSettings()
    {
        InitializeComponent();
        ApplyDefaults();
    }
    
    private void ApplyDefaults()
    {
        Strength.DefaultIfNull = RastaConverterDefaultValues.DefaultDitheringStrength;
        Randomness.DefaultIfNull = RastaConverterDefaultValues.DefaultDitheringRandomness;
        Brightness.DefaultIfNull = RastaConverterDefaultValues.DefaultBrightness;
        Contrast.DefaultIfNull = RastaConverterDefaultValues.DefaultContrast;
        Gamma.DefaultIfNull = RastaConverterDefaultValues.DefaultGamma;
    }
}