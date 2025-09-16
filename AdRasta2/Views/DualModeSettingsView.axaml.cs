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
        FirstSteps.PreventNull();
        FirstSteps.Tag = RastaConverterDefaultValues.DefualtFirstDualSteps;
        AlteringSteps.PreventNull();
        AlteringSteps.Tag = RastaConverterDefaultValues.DefaultAlternatingDualSteps;
        Luma.PreventNull();
        Luma.Tag = RastaConverterDefaultValues.DefaultDualLuma;
        Chroma.PreventNull();
        Chroma.Tag = RastaConverterDefaultValues.DefaultDualChroma;
    }
}