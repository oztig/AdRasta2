using AdRasta2.Extensions;
using AdRasta2.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdRasta2.Views;

public partial class ConfigSettingsView : UserControl
{
    public ConfigSettingsView()
    {
        InitializeComponent();
        ApplyDefaults();
    }

    private void ApplyDefaults()
    {
        RandomSeed.PreventNull();
        RandomSeed.Tag = RastaConverterDefaultValues.DefaultRandomSeed;
        CacheInMB.PreventNull();
        CacheInMB.Tag = RastaConverterDefaultValues.DefaultCacheInMB;
    }
}