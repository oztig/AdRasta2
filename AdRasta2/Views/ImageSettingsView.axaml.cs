using AdRasta2.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdRasta2.Views;

public partial class ImageSettingsView : UserControl
{
    public ImageSettingsView()
    {
        InitializeComponent();
        Strength.DefaultIfNull = RastaConverterDefaultValues.DefaultMaskStrength;
        HeightUpnDown.DefaultIfNull = RastaConverterDefaultValues.DefaultHeight;
    }
}