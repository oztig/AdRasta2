using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AdRasta2.Interfaces;
using AdRasta2.Models;
using AdRasta2.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using ReactiveUI;

namespace AdRasta2.ViewModels;

public class AdRastaMainViewViewModel : ReactiveObject
{
    private Window? _window;
    private Settings _settings = new();
    public string HeadingText { get; set; } = "Ad Rasta v2 - Alpha";

    public SourceData SourceData { get; } = new();

    public ReactiveCommand<Unit, Unit> ShowHelpCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
    public ReactiveCommand<Unit, Unit> PickFileCommand { get; private set; }
    private IFilePickerService _filePickerService;

    public string ViewModelType => GetType().Name;

    public ObservableCollection<int> Sprockets { get; } = new();


    public ObservableCollection<RastaConversion> RastaConversions { get; private set; }

    private RastaConversion? _selectedConversion;

    public RastaConversion? SelectedConversion
    {
        get => _selectedConversion;
        set => this.RaiseAndSetIfChanged(ref _selectedConversion, value);
    }

    public ReactiveCommand<RastaConversion, Unit> PanelClickedCommand { get; }

    public ReactiveCommand<Unit, Unit> NewConversionCommand;
    private int _panelCounter = 4;

    public AdRastaMainViewViewModel(Window window)
    {
        _window = window;
        ShowHelpCommand = ReactiveCommand.CreateFromTask(async () => await ShowHelpMessage());
        ShowAboutCommand = ReactiveCommand.CreateFromTask(async () => await ShowAboutMessage());
        PanelClickedCommand = ReactiveCommand.Create<RastaConversion>(conversion => { ChangeSelected(conversion); });
        NewConversionCommand = ReactiveCommand.Create(AddNewConversion);
        _filePickerService = new FilePickerService(_window);
        
        PopulateSprockets();
        CreateInitialEntry();
    }

    // private async void CheckIniFileExists()
    // {
    //     if (!_settings.CheckIniFileExists())
    //     {
    //         // Show a warning and exit
    //         var messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
    //         {
    //             ContentTitle = "Cannot find Adrasta2.ini file",
    //             ContentMessage = "Unable to find :" + _settings.IniFileLocation,
    //             ButtonDefinitions = new List<ButtonDefinition>
    //             {
    //                 new ButtonDefinition { Name = "Okay" },
    //             },
    //             ShowInCenter = true, WindowStartupLocation = WindowStartupLocation.CenterOwner
    //         });
    //
    //         var result = await messageBox.ShowWindowDialogAsync(_window);
    //         Environment.Exit(-1);
    //     }
    // }

    private void CreateInitialEntry()
    {
        RastaConversions = new ObservableCollection<RastaConversion>
        {
            new RastaConversion("Conversion 1"),
        };

        ChangeSelected(RastaConversions[0]);
    }

    public void SetWindow(Window window)
    {
        _window = window;
    }

    private void PopulateSprockets()
    {
        const int sprocketCount = 7;
        for (int i = 0; i < sprocketCount; i++)
        {
            Sprockets.Add(i); // Used for left and right !
        }
    }

    private void ChangeSelected(RastaConversion conversion)
    {
        var index = RastaConversions.IndexOf(conversion);
        SelectedConversion = conversion;
        // CurrentConversionTitle = conversion.Title;

        SetIsSelected(index);

        // DEBUG
        Console.WriteLine($"Clicked item '{conversion.Title}' at index {index}");
    }

    private void SetIsSelected(int selectedIndex)
    {
        for (int i = 0; i < RastaConversions.Count; i++)
            RastaConversions[i].IsSelected = selectedIndex == i;
    }

    public async void AddNewConversion()
    {
        var userInput = await DialogService.ShowInputDialogAsync("New Conversion",
            "Name:", "", "New Conversion Name", _window);
        if (!(userInput.confirmed ?? false))
            return;

        RastaConversions.Add(new RastaConversion(userInput.value.Trim()));
        ChangeSelected(RastaConversions[^1]);
    }

    private async Task ShowHelpMessage()
    {
        try
        {
            // var result = await Cli.Wrap(_settings.DefaultExecuteCommand)
            //     .WithArguments(SafeCommand.QuoteIfNeeded(_settings.HelpFileLocation))
            //     .WithValidation(CommandResultValidation.None)
            //     .ExecuteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task ShowAboutMessage()
    {
        var dateNow = DateTime.Now;
        var iconPath = new Uri($"avares://AdRasta2/Assets/AdRasta-Icon2.ico");
        var customIcon = new Bitmap(AssetLoader.Open(iconPath));

        var aboutMessage = "RastaConverter by Jakub 'Ilmenit' Debski 2012-" + dateNow.Year + "\n";
        aboutMessage += "AdRasta2 by Nick 'oztig' Pearson\n";
        aboutMessage += "MADS and RC2MCH by Tomasz Biela\n\n";
        aboutMessage += "Special Thanks to:\n";
        aboutMessage += "Arkadiusz Lubaszka for the original RC GUI\n\n";
        aboutMessage += "Developed using JetBrains Rider and Avalonia UI \n";


        var messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = "AdRasta2 (0.1 Bodge It and Scarper Version)",
            ImageIcon = customIcon,
            ContentMessage = aboutMessage,
            ButtonDefinitions = new List<ButtonDefinition>
            {
                new ButtonDefinition { Name = "OK" },
            },
            ShowInCenter = true, WindowStartupLocation = WindowStartupLocation.CenterOwner
        });

        var result = await messageBox.ShowWindowDialogAsync(_window);
    }

    public async void SelectSourceImage()
    {
        SelectedConversion.SourceImagePath = await SelectFiles(FilePickerFileTypes.ImageAll);
    }

    public async void SelectSourceMask()
    {
        SelectedConversion.SourceImageMaskPath = await SelectFiles(FilePickerFileTypes.ImageAll);
    }

    private async Task<string> SelectFiles(FilePickerFileType fileType)
    {
        return await _filePickerService.PickFileAsync(fileType) ?? string.Empty;
    }
}