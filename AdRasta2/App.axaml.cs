using System;
using System.Collections.Generic;
using System.Linq;
using AdRasta2.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AdRasta2.ViewModels;
using AdRasta2.Views;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;

namespace AdRasta2;

public partial class App : Application
{
    private FluentTheme _fluentTheme;
    private readonly List<IStyle> _currentStyleIncludes = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Insert FluentTheme
        Styles.Insert(0, new FluentTheme());

        // Set initial theme variant
        RequestedThemeVariant = ThemeVariant.Light;

        ApplyUserTheme("Default"); // Or load from settings
    }

    public void ApplyUserTheme(string themeName)
    {
        // Clear previous theme fragments
        Resources.MergedDictionaries.Clear();
        // Re-instate once themes are enabled
        // Will need to keep track of which themes to clear etc.
          RestoreDefaults();

        if (themeName == "Default")
        {
            RequestedThemeVariant = ThemeVariant.Default;
            return;
        }

        var baseUri = new Uri("avares://AdRasta2/");
        var brushesUri = new Uri($"avares://AdRasta2/Themes/{themeName}Brushes.axaml");
        var stylesUri = new Uri($"avares://AdRasta2/Themes/{themeName}Styles.axaml");

        TryAddResource(brushesUri, isStyle: false, baseUri);
        TryAddResource(stylesUri, isStyle: true, baseUri);
    }

    private void TryAddResource(Uri uri, bool isStyle, Uri baseUri)
    {
        try
        {
            Console.WriteLine($"Attempting to load resource: {uri}");

            var loaded = AvaloniaXamlLoader.Load(uri);

            if (loaded == null)
            {
                Console.WriteLine($"Load returned null for: {uri}");
                return;
            }

            Console.WriteLine($"Loaded resource type: {loaded.GetType().Name}");

            switch (loaded)
            {
                case ResourceDictionary rd:
                    Application.Current.Resources.MergedDictionaries.Add(rd);
                    Console.WriteLine($"Merged ResourceDictionary from: {uri}");
                    break;

                case Styles styles:
                    Application.Current.Styles.Add(styles);
                    _currentStyleIncludes.Add(styles);
                    Console.WriteLine($"Added Styles to Application.Current.Styles from: {uri}");
                    break;

                default:
                    Console.WriteLine($"Loaded resource is of unexpected type: {loaded.GetType().Name}");
                    break;
            }

            // Confirm that the resource is now in the merged dictionaries
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                Console.WriteLine($"Merged dictionary contains: {dict.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load resource: {uri}. Reason: {ex.Message}");
        }
    }


    private void RestoreDefaults()
    {
        foreach (var style in _currentStyleIncludes)
        {
            Application.Current.Styles.Remove(style);
        }
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        var mainWindow = new MainWindow();

        var filePickerService = new FilePickerService(mainWindow);
        var messageBoxService = new MessageBoxService();

        var viewModel = new MainWindowViewModel(mainWindow, filePickerService, messageBoxService);
        mainWindow.DataContext = viewModel;
        mainWindow.Show();

        base.OnFrameworkInitializationCompleted();
    }
}