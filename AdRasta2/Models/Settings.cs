using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using AdRasta2.Enums;
using AdRasta2.Utils;
using Sini;

namespace AdRasta2.Models;

public class Settings : INotifyPropertyChanged
{
    /// <summary>
    /// Next properties are static,as they are used before Current is initialised!
    /// </summary>
    public static readonly string IniFileLocation =
        Path.Combine(Directory.GetCurrentDirectory().Trim(), "AdRasta2.ini");

    public static bool CheckIniFileExists()
    {
        return File.Exists(IniFileLocation);
    }
    
    public static bool IsWindows => OperatingSystem.IsWindows();
    
    public static RastaConversion ApplicationDebugLog { get; } =
        new RastaConversion("App Debug Log");

    /// End
    
    // ─────────────────────────────────────────────
    // Singleton instance (lazy initialization)
    // ─────────────────────────────────────────────
    private static Settings? _current;
    public static Settings Current => _current ??= new Settings();

    // ─────────────────────────────────────────────
    // Constructor
    // ─────────────────────────────────────────────
    public Settings()
    {
        Console.WriteLine("Settings constructor reached");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Notify([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    

    private string _rastaConverterCommand = string.Empty;

    public string RastaConverterCommand
    {
        get => _rastaConverterCommand;
        set
        {
            _rastaConverterCommand = value;
            Notify();
            Notify(nameof(BaseRastaCommandLocation));
            Notify(nameof(BaseRastaCommand));
        }
    }

    public string BaseRastaCommandLocation => Path.GetDirectoryName(RastaConverterCommand);
    public string BaseRastaCommand => Path.GetFileName(RastaConverterCommand);

    private string _rc2mchCommand = string.Empty;

    public string RC2MCHCommand
    {
        get => _rc2mchCommand;
        set
        {
            _rc2mchCommand = value;
            Notify();
        }
    }

    private string _defaultExecuteCommand = string.Empty;

    public string DefaultExecuteCommand
    {
        get => _defaultExecuteCommand;
        set
        {
            _defaultExecuteCommand = value;
            Notify();
        }
    }

    private string _paletteDirectory = string.Empty;

    public string PaletteDirectory
    {
        get => _paletteDirectory;
        set
        {
            _paletteDirectory = value;
            Notify();
        }
    }

    private string _madsLocation = string.Empty;

    public string MadsLocation
    {
        get => _madsLocation;
        set
        {
            _madsLocation = value;
            Notify();
            Notify(nameof(MadsLocationBaseName));
        }
    }

    public string MadsLocationBaseName => Path.GetFileName(MadsLocation);

    private string _noNameFilesLocation = string.Empty;

    public string NoNameFilesLocation
    {
        get => _noNameFilesLocation;
        set
        {
            _noNameFilesLocation = value;
            Notify();
        }
    }

    private string _dualModeNoNameFilesLocation = string.Empty;

    public string DualModeNoNameFilesLocation
    {
        get => _dualModeNoNameFilesLocation;
        set
        {
            _dualModeNoNameFilesLocation = value;
            Notify();
        }
    }

    private string _helpFileLocation = string.Empty;

    public string HelpFileLocation
    {
        get => _helpFileLocation;
        set
        {
            _helpFileLocation = value;
            Notify();
        }
    }

    private double _rastaConverterVersion;

    public double RastaConverterVersion
    {
        get => _rastaConverterVersion;
        set
        {
            _rastaConverterVersion = value;
            Notify();
        }
    }

    public string NoNameHeader { get; set; } = "no_name.h";
    public string NoNameAsq { get; set; } = "no_name.asq";

    private bool _debugMode;

    public bool DebugMode
    {
        get => _debugMode;
        set
        {
            _debugMode = value;
            Notify();
            Notify(nameof(MaxLogEntries));
        }
    }

    public int MaxLogEntries => DebugMode ? 500 : 100;

    private bool _autoViewPreview;

    public bool AutoViewPreview
    {
        get => _autoViewPreview;
        set
        {
            _autoViewPreview = value;
            Notify();
        }
    }

    private bool _setConversionIcon;

    public bool SetConversionIcon
    {
        get => _setConversionIcon;
        set
        {
            _setConversionIcon = value;
            Notify();
        }
    }

    private string _rcEditCommand = string.Empty;

    public string RCEditCommand
    {
        get => _rcEditCommand;
        set
        {
            _rcEditCommand = value;
            Notify();
            Notify(nameof(BaseRCEditCommandLocation));
            Notify(nameof(BaseRCEditCommand));
        }
    }

    public string BaseRCEditCommandLocation => Path.GetDirectoryName(RCEditCommand);
    public string BaseRCEditCommand => Path.GetFileName(RCEditCommand);

    private bool _dryRunDelete = true;

    public bool DryRunDelete
    {
        get => _dryRunDelete;
        set
        {
            _dryRunDelete = value;
            Notify();
        }
    }

    public void LoadFromIni(RastaConversion loggingConversion)
    {
        if (IsWindows)
            DefaultExecuteCommand = "explorer";
        else if (OperatingSystem.IsLinux())
            DefaultExecuteCommand = "xdg-open";
        else
            DefaultExecuteCommand = "explorer";

        if (!File.Exists(IniFileLocation))
            return;

        try
        {
            var ini = new IniFile(IniFileLocation);

            // RastaConverter related
            RastaConverterCommand = ini.GetStr("RastaConverter", "Location", "Cant Find in ini File");
            HelpFileLocation = ini.GetStr("RastaConverter", "HelpFile", string.Empty);
            PaletteDirectory = ini.GetStr("RastaConverter", "PalettesDir", string.Empty);
            NoNameFilesLocation = ini.GetStr("RastaConverter", "NoNameFilesDir", string.Empty);
            DualModeNoNameFilesLocation = ini.GetStr("RastaConverter", "DualModeNoNameFilesDir", string.Empty);

            // Rataconverter Defaults
            float raw = GetSafeFloat(ini, "RastaConverter.Defaults", "DefaultUnstuckDrift", 0.00001f);
            RastaConverterDefaultValues.DefaultUnstuckDrift = (decimal)raw;
            RastaConverterDefaultValues.DefaultUnstuckAfter =
                ini.GetInt("RastaConverter.Defaults", "DefaultUnstuckAfter", 100000);
            
            // Tools
            RC2MCHCommand = ini.GetStr("Tools", "RC2MCH", string.Empty);
            MadsLocation = ini.GetStr("Tools", "MADS", string.Empty);

            // Debug
            DebugMode = GetSafeBool(ini, "Debug", "DebugMode", false);
            
            //User Preferences
            AutoViewPreview = GetSafeBool(ini, "UserPreferences", "AutoViewPreview", false);

            // Experimental
            SetConversionIcon = GetSafeBool(ini, "Experimental", "SetConversionIcon", false);
            RCEditCommand = ini.GetStr("Experimental", "RCEditCommand", string.Empty);
            DryRunDelete = GetSafeBool(ini, "Experimental", "DryRunDelete", true);
        }
        catch (Exception ex)
        {
            ConversionLogger.LogIfDebug(loggingConversion, ConversionStatus.Debug,
                "Error Reading Settings", ex, "", true);
        }
    }

    private double GetSafeDouble(IniFile ini, string section, string key, double fallback)
    {
        var str = ini.GetStr(section, key);
        return double.TryParse(str, out var result) ? result : fallback;
    }

    private bool GetSafeBool(IniFile ini, string section, string key, bool fallback)
    {
        var str = ini.GetStr(section, key);
        return bool.TryParse(str, out var result) ? result : fallback;
    }

    private float GetSafeFloat(IniFile ini, string section, string key, float fallback)
    {
        var str = ini.GetStr(section, key);
        return float.TryParse(str, out var result) ? result : fallback;
    }

    public void LogSettingValues(RastaConversion loggingConversion)
    {
        var props = GetType().GetProperties(System.Reflection.BindingFlags.Public |
                                            System.Reflection.BindingFlags.Instance);

        foreach (var prop in props)
        {
            try
            {
                var value = prop.GetValue(this);
                var display = value is string s && string.IsNullOrWhiteSpace(s) ? "<empty>" : value?.ToString();
                ConversionLogger.LogIfDebug(loggingConversion, ConversionStatus.Debug,
                    $"Settings.{prop.Name} = {display}", forceDebug: true);
            }
            catch (Exception ex)
            {
                ConversionLogger.LogIfDebug(loggingConversion, ConversionStatus.Debug,
                    $"Settings.{prop.Name} = <error: {ex.Message}>", forceDebug: true);
            }
        }
    }
}