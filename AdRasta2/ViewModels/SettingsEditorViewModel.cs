using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AdRasta2.Attributes;
using AdRasta2.Models;
using AdRasta2.Interfaces;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace AdRasta2.ViewModels;

public class SettingsEditorViewModel : ReactiveObject, INotifyDataErrorInfo
{
    private readonly IFilePickerService _filePicker;
    private readonly IFolderPickerService _folderPicker;
    private readonly Dictionary<string, List<string>> _errors = new();
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }
    public Action? CloseEditorAction { get; set; }



    public SettingsEditorViewModel(IFilePickerService filePicker, IFolderPickerService folderPicker)
    {
        _filePicker = filePicker;
        _folderPicker = folderPicker;

        BrowseForCommand = ReactiveCommand.CreateFromTask<string>(ExecuteBrowseForAsync);
        BrowseForDirectoryCommand = ReactiveCommand.CreateFromTask<string>(BrowseForDirectoryAsync);
        SaveCommand = ReactiveCommand.Create(ExecuteSave);
        CloseCommand = ReactiveCommand.Create(ExecuteClose);
    }

    public ReactiveCommand<string, Unit> BrowseForCommand { get; }
    public ReactiveCommand<string, Unit> BrowseForDirectoryCommand { get; }

    private async Task ExecuteBrowseForAsync(string targetProperty)
    {
        var fileType = new FilePickerFileType("Executable") { Patterns = new[] { "*.exe" } };
        var result = await _filePicker.PickFileAsync(fileType, $"Select file for {targetProperty}");

        if (!string.IsNullOrWhiteSpace(result))
        {
            var property = GetType().GetProperty(targetProperty);
            if (property != null && property.CanWrite && property.PropertyType == typeof(string))
            {
                property.SetValue(this, result);
                ValidateProperty(targetProperty, result);
            }
        }
    }

    private async Task BrowseForDirectoryAsync(string propertyName)
    {
        var folder = await _folderPicker.PickFolderAsync("Select a folder");

        if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
        {
            folder = Path.TrimEndingDirectorySeparator(folder);
            var property = GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite && property.PropertyType == typeof(string))
            {
                property.SetValue(this, folder);
                ValidateProperty(propertyName, folder);
            }
        }
    }

    private bool ValidateProperty(string propertyName, object? value)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(this) { MemberName = propertyName };

        _errors.Remove(propertyName);

        bool isValid = Validator.TryValidateProperty(value, context, results);

        if (!isValid)
        {
            _errors[propertyName] = results.Select(r => r.ErrorMessage ?? "Invalid value").ToList();
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        return isValid;
    }

    public bool HasErrors => _errors.Any();
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return Enumerable.Empty<string>();
        return _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();
    }

    // ─────────────────────────────────────────────
    // Reactive properties with validation
    // ─────────────────────────────────────────────

    private string _helpFileLocation = Settings.Current.HelpFileLocation;

    [Required(ErrorMessage = "Help file location is required.")]
    [FileExists(ErrorMessage = "Help file must exist.")]
    public string HelpFileLocation
    {
        get => _helpFileLocation;
        set
        {
            this.RaiseAndSetIfChanged(ref _helpFileLocation, value);
            ValidateProperty(nameof(HelpFileLocation), value);
        }
    }

    private string _rc2mchCommand = Settings.Current.RC2MCHCommand;

    [Required(ErrorMessage = "RC2MCH command is required.")]
    [FileExists(ErrorMessage = "RC2MCH executable must exist.")]
    public string RC2MCHCommand
    {
        get => _rc2mchCommand;
        set
        {
            this.RaiseAndSetIfChanged(ref _rc2mchCommand, value);
            ValidateProperty(nameof(RC2MCHCommand), value);
        }
    }

    private string _madsLocation = Settings.Current.MadsLocation;

    [Required(ErrorMessage = "MADS location is required.")]
    [FileExists(ErrorMessage = "MADS executable must exist.")]
    public string MadsLocation
    {
        get => _madsLocation;
        set
        {
            this.RaiseAndSetIfChanged(ref _madsLocation, value);
            ValidateProperty(nameof(MadsLocation), value);
        }
    }

    private string _paletteDirectory = Settings.Current.PaletteDirectory;

    [Required(ErrorMessage = "Palette directory is required.")]
    [PathExists(ErrorMessage = "Palette directory must exist.")]
    public string PaletteDirectory
    {
        get => _paletteDirectory;
        set
        {
            this.RaiseAndSetIfChanged(ref _paletteDirectory, value);
            ValidateProperty(nameof(PaletteDirectory), value);
        }
    }

    private string _noNameFilesLocation = Settings.Current.NoNameFilesLocation;

    [Required(ErrorMessage = "NoName files location is required.")]
    [PathExists(ErrorMessage = "NoName directory must exist.")]
    public string NoNameFilesLocation
    {
        get => _noNameFilesLocation;
        set
        {
            this.RaiseAndSetIfChanged(ref _noNameFilesLocation, value);
            ValidateProperty(nameof(NoNameFilesLocation), value);
        }
    }

    private string _dualModeNoNameFilesLocation = Settings.Current.DualModeNoNameFilesLocation;

    [Required(ErrorMessage = "Dual-mode NoName files location is required.")]
    [PathExists(ErrorMessage = "Dual-mode NoName directory must exist.")]
    public string DualModeNoNameFilesLocation
    {
        get => _dualModeNoNameFilesLocation;
        set
        {
            this.RaiseAndSetIfChanged(ref _dualModeNoNameFilesLocation, value);
            ValidateProperty(nameof(DualModeNoNameFilesLocation), value);
        }
    }

    private string _rastaConverterCommand = Settings.Current.RastaConverterCommand;

    [Required(ErrorMessage = "RastaConverter command is required.")]
    [FileExists(ErrorMessage = "RastaConverter executable must exist.")]
    public string RastaConverterCommand
    {
        get => _rastaConverterCommand;
        set
        {
            this.RaiseAndSetIfChanged(ref _rastaConverterCommand, value);
            if (ValidateProperty(nameof(RastaConverterCommand), value))
            {
                PopulateRastConverterPaths();
            }
        }
    }

    private void PopulateRastConverterPaths()
    {
        var baseDir = Path.GetDirectoryName(RastaConverterCommand);

        if (baseDir != null)
        {
            HelpFileLocation = Path.Combine(baseDir, "help.txt");
            PaletteDirectory = Path.Combine(baseDir, "Palettes");
            NoNameFilesLocation = Path.Combine(baseDir, "Generator");
            DualModeNoNameFilesLocation = Path.Combine(baseDir, "GeneratorDual");
        }
    }

    private string _rcEditCommand = Settings.Current.RCEditCommand;

    [RCEditCommandValidator]
    public string RCEditCommand
    {
        get => _rcEditCommand;
        set
        {
            this.RaiseAndSetIfChanged(ref _rcEditCommand, value);
            ValidateProperty(nameof(RCEditCommand), value);
        }
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private string _defaultImageDestination = Settings.Current.DefaultImageDestination;

    [Required(ErrorMessage = "Default image destination is required.")]
    [PathExists(ErrorMessage = "Default image directory must exist.")]
    public string DefaultImageDestination
    {
        get => _defaultImageDestination;
        set
        {
            this.RaiseAndSetIfChanged(ref _defaultImageDestination, value);
            ValidateProperty(nameof(DefaultImageDestination), value);
        }
    }

    // ─────────────────────────────────────────────
    // Non-string properties (no validation applied)
    // ─────────────────────────────────────────────

    private bool _debugMode = Settings.Current.DebugMode;

    public bool DebugMode
    {
        get => _debugMode;
        set => this.RaiseAndSetIfChanged(ref _debugMode, value);
    }

    private bool _autoViewPreview = Settings.Current.AutoViewPreview;

    public bool AutoViewPreview
    {
        get => _autoViewPreview;
        set => this.RaiseAndSetIfChanged(ref _autoViewPreview, value);
    }

    private double _rastaConverterVersion = Settings.Current.RastaConverterVersion;

    public double RastaConverterVersion
    {
        get => _rastaConverterVersion;
        set => this.RaiseAndSetIfChanged(ref _rastaConverterVersion, value);
    }

    private bool _setConversionIcon = Settings.Current.SetConversionIcon;

    public bool SetConversionIcon
    {
        get => _setConversionIcon;
        set => this.RaiseAndSetIfChanged(ref _setConversionIcon, value);
    }

    private bool _dryRunDelete = Settings.Current.DryRunDelete;

    public bool DryRunDelete
    {
        get => _dryRunDelete;
        set => this.RaiseAndSetIfChanged(ref _dryRunDelete, value);
    }

    private decimal _defaultUnstuckDrift = RastaConverterDefaultValues.DefaultUnstuckDrift;

    public decimal DefaultUnstuckDrift
    {
        get => _defaultUnstuckDrift;
        set => this.RaiseAndSetIfChanged(ref _defaultUnstuckDrift, value);
    }

    private int _defaultUnstuckAfter = 1000000;

    public int DefaultUnstuckAfter
    {
        get => _defaultUnstuckAfter;
        set => this.RaiseAndSetIfChanged(ref _defaultUnstuckAfter, value);
    }

    private void ExecuteSave()
    {
        // Apply current view model values to Settings
        Settings.Current.HelpFileLocation = HelpFileLocation;
        Settings.Current.RC2MCHCommand = RC2MCHCommand;
        Settings.Current.MadsLocation = MadsLocation;
        Settings.Current.PaletteDirectory = PaletteDirectory;
        Settings.Current.NoNameFilesLocation = NoNameFilesLocation;
        Settings.Current.DualModeNoNameFilesLocation = DualModeNoNameFilesLocation;
        Settings.Current.RastaConverterCommand = RastaConverterCommand;
        Settings.Current.RCEditCommand = RCEditCommand;
        Settings.Current.DefaultImageDestination = DefaultImageDestination;

        Settings.Current.DebugMode = DebugMode;
        Settings.Current.AutoViewPreview = AutoViewPreview;
        Settings.Current.RastaConverterVersion = RastaConverterVersion;
        Settings.Current.SetConversionIcon = SetConversionIcon;
        Settings.Current.DryRunDelete = DryRunDelete;
        RastaConverterDefaultValues.DefaultUnstuckDrift = DefaultUnstuckDrift;
        RastaConverterDefaultValues.DefaultUnstuckAfter = DefaultUnstuckAfter;

      //  Settings.Save();

        // Optional: close the editor or notify completion
    }

    private void ExecuteClose()
    {
        CloseEditorAction?.Invoke();  
    }
}