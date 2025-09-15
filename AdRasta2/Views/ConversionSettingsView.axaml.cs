using AdRasta2.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdRasta2.Views;

public partial class ConversionSettingsView : UserControl
{
    public ConversionSettingsView()
    {
        InitializeComponent();
        NumSolutions.PreventNull();
        MaxEvals.PreventNull();
    }
}