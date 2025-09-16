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
        Strength.PreventNull();
        Strength.Tag = RastaConverterDefaultValues.DefaultDitheringStrength;
        Randomness.PreventNull();
        Randomness.Tag = RastaConverterDefaultValues.DefaultDitheringRandomness;
        Brightness.PreventNull();
        Brightness.Tag = RastaConverterDefaultValues.DefaultBrightness;
        Contrast.PreventNull();
        Contrast.Tag = RastaConverterDefaultValues.DefaultContrast;
        Gamma.PreventNull();
        Gamma.Tag = RastaConverterDefaultValues.DefaultGamma;
    }
}