using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AdRasta2.Interfaces;
using AdRasta2.Services;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;

namespace AdRasta2.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public AdRastaMainViewViewModel AdRastaMainViewVM { get; }

    private WindowIcon? _icon;

    public WindowIcon? AppIcon
    {
        get => _icon;
        set => this.RaiseAndSetIfChanged(ref _icon, value);
    }

    public MainWindowViewModel(Window window, IFilePickerService filePickerService,
        IFolderPickerService folderPickerService, IMessageBoxService messageBoxService,
        IFileExplorerService fileExplorerService,IconGlyphService iconService)
    {
        AdRastaMainViewVM = new AdRastaMainViewViewModel(window, filePickerService, folderPickerService,
            messageBoxService, fileExplorerService, iconService);
        SetIcon();
    }

    public async Task Initialize(Window window)
    {
        AdRastaMainViewVM.SetWindow(window);
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