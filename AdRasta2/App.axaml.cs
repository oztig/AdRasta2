using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AdRasta2.ViewModels;
using AdRasta2.Views;

namespace AdRasta2;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // public override void OnFrameworkInitializationCompleted()
    // {
    //     if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    //     {
    //         desktop.MainWindow = new MainWindow
    //         {
    //             DataContext = new MainWindowViewModel(),
    //         };
    //     }
    //
    //     base.OnFrameworkInitializationCompleted();
    // }
    
    public override void OnFrameworkInitializationCompleted()
    {
        var mainWindow = new MainWindow();
        mainWindow.DataContext = new MainWindowViewModel(mainWindow); // Pass window directly
        mainWindow.Show();

        base.OnFrameworkInitializationCompleted();
    }
}