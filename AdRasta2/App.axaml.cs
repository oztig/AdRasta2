using AdRasta2.Services;
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