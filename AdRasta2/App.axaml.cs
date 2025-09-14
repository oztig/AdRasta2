using System;
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
        //  RestoreDefaults();

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

    // private void RestoreBaseStyles()
    // {
    //     Application.Current.Styles.Clear();
    //
    //     // Re-add FluentTheme for control templates
    //     Application.Current.Styles.Insert(0, new FluentTheme());
    //
    //     // Re-add icon pack styles
    //     Application.Current.Styles.Add(new StyleInclude(new Uri("avares://AdRasta2/"))
    //     {
    //         Source = new Uri("avares://IconPacks.Avalonia/Icons.axaml")
    //     });
    //
    //     // Add any other base styles here if needed
    // }

    private void RestoreDefaults()
    {
        Application.Current.Styles.Clear();

        // Re-add FluentTheme
        Application.Current.Styles.Add(new FluentTheme());

        // Re-add IconPacks.Avalonia styles
        Application.Current.Styles.Add(new StyleInclude(new Uri("avares://AdRasta2/"))
        {
            Source = new Uri("avares://IconPacks.Avalonia/Icons.axaml")
        });

        // Add custom style
        Application.Current.Styles.Add(CreateHeaderedContentControlStyle());
    }

    private Style CreateHeaderedContentControlStyle()
    {
        return new Style(x => x.OfType<HeaderedContentControl>())
        {
            Setters =
            {
                new Setter(HeaderedContentControl.TemplateProperty, new FuncControlTemplate((control, _) =>
                {
                    var grid = new Grid
                    {
                        RowDefinitions =
                        {
                            new RowDefinition(GridLength.Auto),
                            new RowDefinition(GridLength.Star)
                        },
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(GridLength.Auto),
                            new ColumnDefinition(GridLength.Star)
                        }
                    };

                    var headerBorder = new Border
                    {
                        ZIndex = 1,
                        Padding = new Thickness(5, 0, 5, 0),
                        Margin = new Thickness(5, 0, 0, 0),
                        Child = new TextBlock
                        {
                            FontWeight = FontWeight.Bold,
                            [!TextBlock.TextProperty] = control[!HeaderedContentControl.HeaderProperty]
                        }
                    };
                    headerBorder[!Border.BackgroundProperty] = new Binding
                    {
                        Source = Application.Current.Resources,
                        Path = "SystemControlBackgroundAltHighBrush"
                    };

                    var contentBorder = new Border
                    {
                        Padding = new Thickness(0, 5, 0, 0),
                        CornerRadius = new CornerRadius(4),
                        Margin = new Thickness(0, 10, 0, 0),
                        BorderThickness = new Thickness(1),
                        Child = new ContentPresenter
                        {
                            Name = "PART_ContentPresenter",
                            Padding = new Thickness(8),
                            [!ContentPresenter.ContentProperty] = control[!HeaderedContentControl.ContentProperty]
                        },
                        [Grid.RowSpanProperty] = 2,
                        [Grid.ColumnSpanProperty] = 2
                    };
                    contentBorder[!Border.BorderBrushProperty] = new Binding
                    {
                        Source = Application.Current.Resources,
                        Path = "SystemControlForegroundBaseMediumBrush"
                    };

                    grid.Children.Add(headerBorder);
                    grid.Children.Add(contentBorder);

                    return grid;
                }))
            }
        };
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