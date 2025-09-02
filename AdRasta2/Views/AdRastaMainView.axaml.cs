using AdRasta2.Models;
using AdRasta2.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;

namespace AdRasta2.Views;

public partial class AdRastaMainView : UserControl
{
    public AdRastaMainView()
    {
        InitializeComponent();
        DataContext = new AdRastaMainViewViewModel(new Window());
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if ((sender as Control)?.DataContext is RastaConversion conversion)
        {
            var fred = "sleep stuff";
        }
    }
}