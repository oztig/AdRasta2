using ReactiveUI;
using System.Reactive;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using AdRasta2.Models;
using AdRasta2.Interfaces;
using Avalonia.Platform.Storage;

namespace AdRasta2.ViewModels;

public class SettingsEditorViewModel : ReactiveObject
{
    private readonly IFilePickerService _filePicker;
    private readonly IFolderPickerService _folderPicker;

    public SettingsEditorViewModel(IFilePickerService filePicker,IFolderPickerService folderPicker)
    {
        _filePicker = filePicker;
        _folderPicker = folderPicker;

        BrowseForCommand = ReactiveCommand.CreateFromTask<string>(ExecuteBrowseForAsync);
        BrowseForDirectoryCommand = ReactiveCommand.CreateFromTask<string>(BrowseForDirectoryAsync);
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
            }
            else
            {
                // Optional: log or throw if property is invalid
            }
        }
    }
    
    private async Task BrowseForDirectoryAsync(string propertyName)
    {
        var folder = await _folderPicker.PickFolderAsync("Select a folder");
        if (folder == null) return;

        var property = GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(this, folder);
        }
    }


    // ─────────────────────────────────────────────
    // Reactive properties initialized from Settings.Current
    // ─────────────────────────────────────────────

    private string _helpFileLocation = Settings.Current.HelpFileLocation;
    public string HelpFileLocation
    {
        get => _helpFileLocation;
        set => this.RaiseAndSetIfChanged(ref _helpFileLocation, value);
    }

    private string _rc2mchCommand = Settings.Current.RC2MCHCommand;
    public string RC2MCHCommand
    {
        get => _rc2mchCommand;
        set => this.RaiseAndSetIfChanged(ref _rc2mchCommand, value);
    }

    private string _madsLocation = Settings.Current.MadsLocation;
    public string MadsLocation
    {
        get => _madsLocation;
        set => this.RaiseAndSetIfChanged(ref _madsLocation, value);
    }

    private string _paletteDirectory = Settings.Current.PaletteDirectory;
    public string PaletteDirectory
    {
        get => _paletteDirectory;
        set => this.RaiseAndSetIfChanged(ref _paletteDirectory, value);
    }

    private string _noNameFilesLocation = Settings.Current.NoNameFilesLocation;
    public string NoNameFilesLocation
    {
        get => _noNameFilesLocation;
        set => this.RaiseAndSetIfChanged(ref _noNameFilesLocation, value);
    }

    private string _dualModeNoNameFilesLocation = Settings.Current.DualModeNoNameFilesLocation;
    public string DualModeNoNameFilesLocation
    {
        get => _dualModeNoNameFilesLocation;
        set => this.RaiseAndSetIfChanged(ref _dualModeNoNameFilesLocation, value);
    }

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

    private string _rastaConverterCommand = Settings.Current.RastaConverterCommand;
    public string RastaConverterCommand
    {
        get => _rastaConverterCommand;
        set => this.RaiseAndSetIfChanged(ref _rastaConverterCommand, value);
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

    private string _rcEditCommand = Settings.Current.RCEditCommand;
    public string RCEditCommand
    {
        get => _rcEditCommand;
        set => this.RaiseAndSetIfChanged(ref _rcEditCommand, value);
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
    
   private  int _defaultUnstuckAfter = 1000000;

   public int DefaultUnstuckAfter
   {
       get => _defaultUnstuckAfter;
       set => this.RaiseAndSetIfChanged(ref _defaultUnstuckAfter, value);
   }
   
}
