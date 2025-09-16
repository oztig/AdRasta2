using AdRasta2.Extensions;
using AdRasta2.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdRasta2.Views;

public partial class ConversionSettingsView : UserControl
{
    public ConversionSettingsView()
    {
        InitializeComponent();
        ApplyDefaults();
    }

    private void ApplyDefaults()
    {
        NumSolutions.PreventNull();
        NumSolutions.Tag = RastaConverterDefaultValues.DefaultSolutionHistoryLength;
        MaxEvals.PreventNull();
        MaxEvals.Tag = RastaConverterDefaultValues.DefaultMaxEvaluations;
    }
}