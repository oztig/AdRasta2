using AdRasta2.Models;
using AdRasta2.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
    private async void SourceImage_DoubleTapped(object sender, RoutedEventArgs e)
    {
        if (DataContext is AdRastaMainViewViewModel vm)
        {
            await vm.ViewSourceImageAsync();
        }
    }
    
}