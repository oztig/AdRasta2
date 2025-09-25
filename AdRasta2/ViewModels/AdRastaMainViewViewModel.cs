using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using MsBox.Avalonia.ViewModels.Commands;
using ReactiveUI;

namespace AdRasta2.ViewModels;

public class AdRastaMainViewViewModel : ReactiveObject
{
    private Window? _window;

    // private Settings _settings = new();
    private int _selectedIndex = 0;
    public object Dummy => null;

    public string HeadingText { get; set; } = "Ad Rasta v2 - Alpha";

    public Action ScrollToLatestLogEntry { get; set; }

    private double _tempFontSize = 12;

    public double TempFontSize
    {
        get => _tempFontSize;
        set
        {
            _tempFontSize = value;
            this.RaisePropertyChanged();
        }
    }

    private double _userFontSize = 12;

    public double UserFontSize
    {
        get => _userFontSize;
        set
        {
            if (_userFontSize != value)
            {
                _userFontSize = value;
                this.RaisePropertyChanged();
                Application.Current.Resources["GlobalFontSize"] = UserFontSize;
            }
        }
    }


    // Button Colours
    public ConversionStatus PreviewButtonColour => ConversionStatus.PreviewGenerated;
    public ConversionStatus MCHButtonColour => ConversionStatus.MCHGenerated;
    public ConversionStatus XexButtonColour => ConversionStatus.ExecutableGenerated;
    public ConversionStatus ConversionButtonColour => ConversionStatus.ConversionComplete;
    public ConversionStatus ContinueButtonColour => ConversionStatus.ConversionStarted;

    public SourceData SourceData { get; } = new();
    public RastaConverterDefaultValues ConverterDefaults { get; }


    public ReactiveCommand<string, Unit> SwitchThemeCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowHelpCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
    public ReactiveCommand<Unit, Unit> PickFileCommand { get; private set; }
    private readonly IFilePickerService _filePickerService;
    private readonly IFolderPickerService _folderPickerService;
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
        IFolderPickerService folderPickerService,
        IMessageBoxService messageBoxService)
    {
        _window = window;
        _filePickerService = filePickerService;
        _folderPickerService = folderPickerService;
        _messageBoxService = messageBoxService;
        SwitchThemeCommand = ReactiveCommand.Create<string>(SwitchTheme);
        ShowHelpCommand = ReactiveCommand.CreateFromTask(async () => await ShowHelpMessage());
        ShowAboutCommand = ReactiveCommand.CreateFromTask(async () => await ShowAboutMessage());
        PanelClickedCommand = ReactiveCommand.Create<RastaConversion>(conversion => { ChangeSelected(conversion); });
        NewConversionCommand = ReactiveCommand.Create(AddNewConversion);

        PopulateSprockets();
        CreateInitialEntry();
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
        SelectedConversion?.ScrollToLatestLogEntry?.Invoke();

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
            var result = await Cli.Wrap(Settings.DefaultExecuteCommand)
                .WithArguments(SafeCommand.QuoteIfNeeded(Settings.HelpFileLocation))
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void SwitchTheme(string themeName)
    {
        try
        {
            ((App)Application.Current).ApplyUserTheme(themeName);
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
        aboutMessage += "AdRasta2 (One Louder) by Nick 'oztig' Pearson\n";
        aboutMessage += "MADS and RC2MCH by Tomasz Biela\n\n";
        aboutMessage += "Special Thanks to:\n";
        aboutMessage += "Arkadiusz Lubaszka for the original RC GUI\n\n";
        aboutMessage += "Developed using JetBrains Rider and Avalonia UI \n";

        var messageBox = MessageBoxManager.GetMessageBoxCustom(new MessageBoxCustomParams
        {
            ContentTitle = "AdRasta2 (Beta 1 - Bodge It and Scarper Version)",
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


    public async void SelectDestinationFoler()
    {
        SelectedConversion.DestinationFilePath = await SelectFolder();
    }

    public async void CreateDestinationFoler()
    {
        var NewFolder =
            await DialogService.ShowInputDialogAsync("Create New Folder", "Folder Name", "", "New Folder Name",
                _window);

        if (NewFolder.confirmed.Value && NewFolder.value.Trim() != string.Empty)
        {
            var folderToCreate = Path.Combine(SelectedConversion.DestinationFilePath, NewFolder.value.Trim());
            if (FileUtils.CreateFolder(folderToCreate))
                SelectedConversion.DestinationFilePath = folderToCreate;
        }
    }

    public async void SelectSourceImage()
    {
        SelectedConversion.SourceImagePath = await SelectFiles(FilePickerFileTypes.ImageAll);
    }

    public async void SelectSourceMask()
    {
        SelectedConversion.SourceImageMaskPath = await SelectFiles(FilePickerFileTypes.ImageAll);
    }

    public async void SelectRegisterOnOffFile()
    {
        SelectedConversion.RegisterOnOffFilePath = await SelectFiles(FilePickerFileTypes.TextPlain);
    }

    public async void ResetBrightness()
    {
        SelectedConversion.Brightness = RastaConverterDefaultValues.DefaultBrightness;
    }

    public async void ResetContrast()
    {
        SelectedConversion.Contrast = RastaConverterDefaultValues.DefaultContrast;
    }

    public async void ResetGamma()
    {
        SelectedConversion.Gamma = RastaConverterDefaultValues.DefaultGamma;
    }

    private async Task<string> SelectFolder()
    {
        return await _folderPickerService.PickFolderAsync("Select Destination Folder") ?? string.Empty;
    }

    private async Task<string> SelectFiles(FilePickerFileType fileType)
    {
        return await _filePickerService.PickFileAsync(fileType) ?? string.Empty;
    }

    private async Task SetPreviewImage(bool finalImage)
    {
        SelectedConversion.ImagePreviewPath = null;

        if (finalImage)
        {
            if (SelectedConversion.DualFrameMode)
                SelectedConversion.ImagePreviewPath = Path.Combine(SelectedConversion.DestinationFilePath,
                    RastaConverterDefaultValues.DefaultDualModeConvertedImageName);
            else
                SelectedConversion.ImagePreviewPath = Path.Combine(SelectedConversion.DestinationFilePath,
                    RastaConverterDefaultValues.DefaultConvertedImageName);
        }
        else
        {
            if (SelectedConversion.DualFrameMode)
                SelectedConversion.ImagePreviewPath = null;
            else
                SelectedConversion.ImagePreviewPath = Path.Combine(SelectedConversion.DestinationFilePath,
                    RastaConverterDefaultValues.DefaultDestintionName);
        }
    }

    public async Task PreviewImage()
    {
        SelectedConversion.PreviewHeaderTitle = "Preview";

        SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.PreviewStarted, "");
        SelectedConversion.ImagePreviewPath = string.Empty;
        await RastaConverter.ExecuteCommand(true, false, SelectedConversion);
        await SetPreviewImage(false);

        SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.PreviewGenerated,
            "(" + SelectedConversion.PreviewImageColoursText + ")");

        // await ViewImage(viewFileName);
    }

    public async Task ConvertImage()
    {
        SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.ConversionStarted, "");
        await RastaConverter.ExecuteCommand(false, false, SelectedConversion);
        await SetPreviewImage(true);
        SelectedConversion.PreviewHeaderTitle = "Result";
        SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.ConversionComplete,
            "(" + SelectedConversion.PreviewImageColoursText + ")");
    }

    public async Task ContinueConvertImage()
    {
        SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.ConversionStarted, "");
        await RastaConverter.ExecuteCommand(false, true, SelectedConversion);
        await SetPreviewImage(true);
        SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.ConversionComplete,
            "(" + SelectedConversion.PreviewImageColoursText + ")");
    }

    public async Task GenerateExecutable()
    {
        var ret = AdRastaStatus.Success;

        if (SelectedConversion?.ExecutableFileName.Trim() == string.Empty)
        {
            if (SelectedConversion.DualFrameMode)
                SelectedConversion.ExecutableFileName = RastaConverterDefaultValues.DefaultDualModeConvertedImageName;
            else
                SelectedConversion.ExecutableFileName = Path
                    .GetFileNameWithoutExtension(RastaConverterDefaultValues.DefaultConvertedImageName);
        }

        var userInput = await DialogService.ShowInputDialogAsync("Executable Name (no .xex extension required)",
            "Executable Name", SelectedConversion.ExecutableFileName, "destination name, no .xex required", _window);
        if (!(userInput.confirmed ?? false))
            return;

        SelectedConversion.ExecutableFileName = userInput.value;

        ret = await Mads.GenerateExecutableFileAsync(SelectedConversion);
        if (ret == AdRastaStatus.Success)
        {
            SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.ExecutableGenerated,
                $"({SelectedConversion.PreviewImageColoursText})");

            ret = await Atari800.RunExecutableAsync(SelectedConversion);
        }
    }

    public async Task GenerateMCH()
    {
        var ret = AdRastaStatus.UnknownError;
        var MCHfile = string.Empty;

        if (SelectedConversion.DualFrameMode)
            MCHfile = Path.Combine(SelectedConversion.DestinationDirectory,
                RastaConverterDefaultValues.DefaultDualModeConvertedImageName);
        else
            MCHfile = Path.Combine(SelectedConversion.DestinationDirectory,
                RastaConverterDefaultValues.DefaultConvertedImageName);

        ret = await rc2mch.GenerateMCH(Settings.RC2MCHCommand, MCHfile);
        if (ret == AdRastaStatus.Success)
            SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.MCHGenerated,
                $"({SelectedConversion.PreviewImageColoursText})");
    }

    public async Task ViewPreviewImageAsync()
    {
        await ImageUtils.ViewImage(SelectedConversion.ImagePreviewPath);
    }

    public async Task ApplyFontSize()
    {
        UserFontSize = TempFontSize;
        Application.Current.Resources["GlobalFontSize"] = UserFontSize;
    }
}