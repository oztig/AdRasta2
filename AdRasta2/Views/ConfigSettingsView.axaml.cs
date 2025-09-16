using AdRasta2.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdRasta2.Views;

public partial class ConfigSettingsView : UserControl
{
    public ConfigSettingsView()
    {
        InitializeComponent();
        RandomSeed.PreventNull();
        CacheInMB.PreventNull();
    }
}