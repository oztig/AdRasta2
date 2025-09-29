using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
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
    
    public bool IsDebugEnabled
    {
        get => Settings.DebugMode;
        set
        {
            Settings.DebugMode = value;
            ConversionLogger.Log(null, ConversionStatus.Debug, $"Debug mode set to: {value}");
            this.RaisePropertyChanged();
        }
    }

    private int _selectedIndex = 0;

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

    public ReactiveCommand<string, Unit> SwitchThemeCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowHelpCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
    private readonly IFilePickerService _filePickerService;
    private readonly IFolderPickerService _folderPickerService;
    private readonly IMessageBoxService _messageBoxService;

    public ObservableCollection<int> Sprockets { get; } = new();
    public ObservableCollection<RastaConversion> RastaConversions { get; private set; }

    private RastaConversion? _selectedConversion;

    public RastaConversion? SelectedConversion
    {
        get => _selectedConversion;
        set => this.RaiseAndSetIfChanged(ref _selectedConversion, value);
    }

    public ReactiveCommand<RastaConversion, Unit> PanelClickedCommand { get; }

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
        UpdateDuplicateDestinationFlags();
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
        {
            SelectedConversion.PopulateDefaultValues();
            UpdateDuplicateDestinationFlags();
        }
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
            UpdateDuplicateDestinationFlags();
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
        UpdateDuplicateDestinationFlags();
    }

    private void UpdateDuplicateDestinationFlags()
    {
        // Reset all flags
        foreach (var conversion in RastaConversions)
            conversion.DuplicateImageDestination = false;

        // Group by normalized path, ignoring empty
        var groups = RastaConversions
            .Where(c => !string.IsNullOrWhiteSpace(c.DestinationFilePath))
            .GroupBy(c => c.DestinationFilePath.TrimEnd('\\'));

        // Mark duplicates
        foreach (var group in groups)
        {
            if (group.Count() > 1)
            {
                foreach (var conversion in group)
                    conversion.DuplicateImageDestination = true;
            }
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

    public async Task PreviewImage()
    {
        SelectedConversion.PreviewHeaderTitle = "Preview";
        SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.PreviewStarted, "");
        SelectedConversion.ImagePreviewPath = string.Empty;

        var result = await RastaConverter.ExecuteCommand(true, false, SelectedConversion);

        if (result.Status != AdRastaStatus.Success || result.ExitCode != 1)
            return;

        await result.Conversion.SetPreviewImage(false);
        ConversionLogger.Log(result.Conversion, ConversionStatus.PreviewGenerated,
            $"({result.Conversion.PreviewImageColoursText})");
    }

    // public async Task PreviewImage()
    // {
    //     SelectedConversion.PreviewHeaderTitle = "Preview";
    //
    //     SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.PreviewStarted, "");
    //     SelectedConversion.ImagePreviewPath = string.Empty;
    //     var toUpdate = await RastaConverter.ExecuteCommand(true, false, SelectedConversion);
    //
    //     await toUpdate.SetPreviewImage(false);
    //     toUpdate.Statuses.AddEntry(DateTime.Now, ConversionStatus.PreviewGenerated,
    //         "(" + toUpdate.PreviewImageColoursText + ")");
    // }

    public async Task ConvertImage()
    {
        SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.ConversionStarted, "");

        var result = await RastaConverter.ExecuteCommand(false, false, SelectedConversion);

        if (result.Status != AdRastaStatus.Success || result.ExitCode != 0)
            return;

        await result.Conversion.SetPreviewImage(true);

        ConversionLogger.Log(result.Conversion, ConversionStatus.ConversionComplete,
            $"({result.Conversion.PreviewImageColoursText})");
    }

    // public async Task ConvertImage()
    // {
    //     SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.ConversionStarted, "");
    //
    //     var toUpdate = await RastaConverter.ExecuteCommand(false, false, SelectedConversion);
    //     await toUpdate.SetPreviewImage(true);
    //     toUpdate.PreviewHeaderTitle = "Result";
    //
    //     toUpdate.Statuses.AddEntry(DateTime.Now, ConversionStatus.ConversionComplete,
    //         $"({toUpdate.PreviewImageColoursText})");
    // }

    public async Task ContinueConvertImage()
    {
        SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.ConversionStarted, "");

        var result = await RastaConverter.ExecuteCommand(false, true, SelectedConversion);

        if (result.Status != AdRastaStatus.Success || result.ExitCode != 0)
            return;

        await result.Conversion.SetPreviewImage(true);

        ConversionLogger.Log(result.Conversion, ConversionStatus.ConversionComplete,
            $"({result.Conversion.PreviewImageColoursText})");
    }

    // public async Task ContinueConvertImage()
    // {
    //     SelectedConversion.Statuses.AddEntry(DateTime.Now, ConversionStatus.ConversionStarted, "");
    //
    //     var toUpdate = await RastaConverter.ExecuteCommand(false, true, SelectedConversion);
    //     await toUpdate.SetPreviewImage(true);
    //
    //     toUpdate.Statuses.AddEntry(DateTime.Now, ConversionStatus.ConversionComplete,
    //         "(" + toUpdate.PreviewImageColoursText + ")");
    // }

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
            ret = await Atari800.RunExecutableAsync(SelectedConversion);
        }
    }

    public async Task GenerateMCH()
    {
        var toUpdate = await rc2mch.GenerateMCH(SelectedConversion);
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

    public async void CopyLogToClipboardAsync()
    {
        var logText = string.Join(Environment.NewLine,
            SelectedConversion.Statuses.Select(s => s.Message));

        if (!string.IsNullOrWhiteSpace(logText))
            await _window.Clipboard.SetTextAsync(logText);
    }


    public RastaConversion? FindConversionByProcessId(int processId)
    {
        if (processId <= 0)
            return null;

        return RastaConversions.FirstOrDefault(c => c.ProcessID == processId);
    }
}