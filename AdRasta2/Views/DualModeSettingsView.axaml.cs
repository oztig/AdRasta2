using AdRasta2.Extensions;
using AdRasta2.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdRasta2.Views;

public partial class DualModeSettingsView : UserControl
{
    public DualModeSettingsView()
    {
        InitializeComponent();
        ApplyDefaults();
    }

    private void ApplyDefaults()
    {
        FirstSteps.DefaultIfNull = RastaConverterDefaultValues.DefualtFirstDualSteps;
        AlteringSteps.DefaultIfNull = RastaConverterDefaultValues.DefaultAlternatingDualSteps;
        Luma.DefaultIfNull = RastaConverterDefaultValues.DefaultDualLuma;
        Chroma.DefaultIfNull = RastaConverterDefaultValues.DefaultDualChroma;
    }
}