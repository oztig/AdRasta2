using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AdRasta2.Enums;
using AdRasta2.Interfaces;
using AdRasta2.Models;
using AdRasta2.Services;
using AdRasta2.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using CliWrap;
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
    private int _selectedIndex = 0;
    public string HeadingText { get; set; } = "Ad Rasta v2 - Alpha";

    public SourceData SourceData { get; } = new();

    public ReactiveCommand<Unit, Unit> ShowHelpCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
    public ReactiveCommand<Unit, Unit> PickFileCommand { get; private set; }
    private readonly IFilePickerService _filePickerService;
    private readonly IMessageBoxService _messageBoxService;

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

    public AdRastaMainViewViewModel(Window window, IFilePickerService filePickerService,
        IMessageBoxService messageBoxService)
    {
        _window = window;
        _filePickerService = filePickerService;
        _messageBoxService = messageBoxService;
        ShowHelpCommand = ReactiveCommand.CreateFromTask(async () => await ShowHelpMessage());
        ShowAboutCommand = ReactiveCommand.CreateFromTask(async () => await ShowAboutMessage());
        PanelClickedCommand = ReactiveCommand.Create<RastaConversion>(conversion => { ChangeSelected(conversion); });
        NewConversionCommand = ReactiveCommand.Create(AddNewConversion);

        PopulateSprockets();
        CreateInitialEntry();
        
        // DEBUG
        var newStat = new StatusEntry
        {
            Status = ConversionStatus.PreviewGenerated,
            Timestamp = DateTime.Now
        };
        SelectedConversion.Statuses.Add(newStat);
        
        newStat = new StatusEntry
        {
            Status = ConversionStatus.ExecutableGenerated,
            Timestamp = DateTime.Now.AddDays(-3)
        };
        SelectedConversion.Statuses.Add(newStat);
    }

    private void CreateInitialEntry()
    {
        RastaConversions = new ObservableCollection<RastaConversion>
        {
            new RastaConversion("New Conversion"),
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
            Sprockets.Add(i); // Used for left and right !
    }

    private void ChangeSelected(RastaConversion conversion)
    {
        _selectedIndex = RastaConversions.IndexOf(conversion);
        SelectedConversion = conversion;
        SetIsSelected(_selectedIndex);

        // DEBUG
        Console.WriteLine($"Clicked item '{conversion.Title}' at index {_selectedIndex}");
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

    public async void HFlipSourceImage()
    {
        await _messageBoxService.ShowInfoAsync("H-Flip Source Image", "Not Implemented Yet!");
    }

    public async void HFlipMaskImage()
    {
        await _messageBoxService.ShowInfoAsync("H-Flip Mask Image", "Not Implemented Yet!");
    }

    private async Task ShowHelpMessage()
    {
        try
        {
            var result = await Cli.Wrap(_settings.DefaultExecuteCommand)
                .WithArguments(SafeCommand.QuoteIfNeeded(_settings.HelpFileLocation))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task ResetCurrentConversionValues()
    {
        var result = await _messageBoxService.ShowConfirmationAsync("Reset all settings?",
            "This will reset all values of the currently selected conversion." + Environment.NewLine + "Are You Sure?",
            Icon.Question);

        if (result.ToLower() == "okay")
            SelectedConversion.PopulateDefaultValues();
    }

    public async Task RemoveCurrentConversion()
    {
        if (RastaConversions.Count <= 1)
        {
            await _messageBoxService.ShowInfoAsync("Cannot Remove!", "Cannot Remove the initial conversion");
            return;
        }

        var result = await _messageBoxService.ShowConfirmationAsync("Remove Selected Conversion?",
            "This will remove the currently selected conversion." + Environment.NewLine + " Are You Sure?",
            Icon.Question);

        if (result.ToLower() == "okay")
        {
            RastaConversions.Remove(SelectedConversion);
            ChangeSelected(RastaConversions[^1]);
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