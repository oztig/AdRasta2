using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;

namespace AdRasta2.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public AdRastaMainViewViewModel AdRastaMainViewVM { get; } = new AdRastaMainViewViewModel();

    private WindowIcon? _icon;
    public WindowIcon? AppIcon
    {
        get => _icon;
        set => this.RaiseAndSetIfChanged(ref _icon, value);
    }
    
    public MainWindowViewModel()
    {
        SetIcon();
    }
    
    private void SetIcon()
    {
        Uri iconPath;

        try
        {
            if (Debugger.IsAttached)
                iconPath = new Uri($"avares://AdRasta2/Assets/AdRasta-Debug.png");
            else if (OperatingSystem.IsWindows())
                iconPath = new Uri($"avares://AdRasta2/Assets/AdRasta-Icon2.ico");
            else
                iconPath = new Uri($"avares://AdRasta2/Assets/AdRasta-Icon2.png");

            AppIcon = new WindowIcon(new Bitmap(AssetLoader.Open(iconPath)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}