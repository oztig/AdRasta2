using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Utils;
using ReactiveUI;
using Sini;

namespace AdRasta2.Models;

public class Settings : ReactiveObject
{
    public static readonly string IniFileLocation =
        Path.Combine(Directory.GetCurrentDirectory().Trim(), "AdRasta2.ini");

    public static bool CheckIniFileExists() => File.Exists(IniFileLocation);
    public static bool IsWindows => OperatingSystem.IsWindows();

    public static RastaConversion ApplicationDebugLog { get; } =
        new RastaConversion("App Debug Log");

    private static Settings? _current;
    public static Settings Current => _current ??= new Settings();

    public Settings()
    {
        Console.WriteLine("Settings constructor reached");
    }

    private string _rastaConverterCommand = string.Empty;

    public string RastaConverterCommand
    {
        get => _rastaConverterCommand;
        set => this.RaiseAndSetIfChanged(ref _rastaConverterCommand, value);
    }

    public string BaseRastaCommandLocation => Path.GetDirectoryName(RastaConverterCommand);
    public string BaseRastaCommand => Path.GetFileName(RastaConverterCommand);

    private string _rc2mchCommand = string.Empty;

    public string RC2MCHCommand
    {
        get => _rc2mchCommand;
        set => this.RaiseAndSetIfChanged(ref _rc2mchCommand, value);
    }

    private string _defaultExecuteCommand = string.Empty;

    public string DefaultExecuteCommand
    {
        get => _defaultExecuteCommand;
        set => this.RaiseAndSetIfChanged(ref _defaultExecuteCommand, value);
    }

    private string _paletteDirectory = string.Empty;

    public string PaletteDirectory
    {
        get => _paletteDirectory;
        set => this.RaiseAndSetIfChanged(ref _paletteDirectory, value);
    }

    private string _madsLocation = string.Empty;

    public string MadsLocation
    {
        get => _madsLocation;
        set => this.RaiseAndSetIfChanged(ref _madsLocation, value);
    }

    public string MadsLocationBaseName => Path.GetFileName(MadsLocation);

    private string _noNameFilesLocation = string.Empty;

    public string NoNameFilesLocation
    {
        get => _noNameFilesLocation;
        set => this.RaiseAndSetIfChanged(ref _noNameFilesLocation, value);
    }

    private string _dualModeNoNameFilesLocation = string.Empty;

    public string DualModeNoNameFilesLocation
    {
        get => _dualModeNoNameFilesLocation;
        set => this.RaiseAndSetIfChanged(ref _dualModeNoNameFilesLocation, value);
    }

    private string _helpFileLocation = string.Empty;

    public string HelpFileLocation
    {
        get => _helpFileLocation;
        set => this.RaiseAndSetIfChanged(ref _helpFileLocation, value);
    }

    private double _rastaConverterVersion;

    public double RastaConverterVersion
    {
        get => _rastaConverterVersion;
        set => this.RaiseAndSetIfChanged(ref _rastaConverterVersion, value);
    }

    public string NoNameHeader { get; set; } = "no_name.h";
    public string NoNameAsq { get; set; } = "no_name.asq";

    private bool _debugMode;

    public bool DebugMode
    {
        get => _debugMode;
        set => this.RaiseAndSetIfChanged(ref _debugMode, value);
    }

    public int MaxLogEntries => DebugMode ? 500 : 100;

    private bool _autoViewPreview;

    public bool AutoViewPreview
    {
        get => _autoViewPreview;
        set => this.RaiseAndSetIfChanged(ref _autoViewPreview, value);
    }

    private bool _setConversionIcon;

    public bool SetConversionIcon
    {
        get => _setConversionIcon;
        set => this.RaiseAndSetIfChanged(ref _setConversionIcon, value);
    }

    private string _rcEditCommand = string.Empty;

    public string RCEditCommand
    {
        get => _rcEditCommand;
        set => this.RaiseAndSetIfChanged(ref _rcEditCommand, value);
    }

    public string BaseRCEditCommandLocation => Path.GetDirectoryName(RCEditCommand);
    public string BaseRCEditCommand => Path.GetFileName(RCEditCommand);

    private bool _dryRunDelete = true;

    public bool DryRunDelete
    {
        get => _dryRunDelete;
        set => this.RaiseAndSetIfChanged(ref _dryRunDelete, value);
    }

    private string _defaultImageDestination = string.Empty;

    public string DefaultImageDestination
    {
        get => _defaultImageDestination;
        set => this.RaiseAndSetIfChanged(ref _defaultImageDestination, value);
    }

    public void LoadFromIni(RastaConversion loggingConversion)
    {
        DefaultExecuteCommand = IsWindows ? "explorer" :
            OperatingSystem.IsLinux() ? "xdg-open" : "explorer";

        if (!File.Exists(IniFileLocation))
            return;

        try
        {
            var ini = new IniFile(IniFileLocation);

            RastaConverterCommand = ini.GetStr("RastaConverter", "Location", "Cant Find in ini File");
            HelpFileLocation = ini.GetStr("RastaConverter", "HelpFile", string.Empty);
            PaletteDirectory = ini.GetStr("RastaConverter", "PalettesDir", string.Empty);
            NoNameFilesLocation = ini.GetStr("RastaConverter", "NoNameFilesDir", string.Empty);
            DualModeNoNameFilesLocation = ini.GetStr("RastaConverter", "DualModeNoNameFilesDir", string.Empty);

            float raw = GetSafeFloat(ini, "RastaConverter.Defaults", "DefaultUnstuckDrift", 0.00001f);
            RastaConverterDefaultValues.DefaultUnstuckDrift = (decimal)raw;
            RastaConverterDefaultValues.DefaultUnstuckAfter =
                ini.GetInt("RastaConverter.Defaults", "DefaultUnstuckAfter", 100000);
             RastaConverterDefaultValues.DefaultPreColourDistance = ini.GetStr("RastaConverter.Defaults", "DefaultPreColourDistance", "ciede");
             RastaConverterDefaultValues.DefaultColourDistance = ini.GetStr("RastaConverter.Defaults", "DefaultColourDistance", "yuv");
            

            RC2MCHCommand = ini.GetStr("Tools", "RC2MCH", string.Empty);
            MadsLocation = ini.GetStr("Tools", "MADS", string.Empty);

            DebugMode = GetSafeBool(ini, "Debug", "DebugMode", false);

            AutoViewPreview = GetSafeBool(ini, "UserPreferences", "AutoViewPreview", false);
            DefaultImageDestination = ini.GetStr("UserPreferences", "DefaultImageDestination", string.Empty);

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
                if (prop.Name is "Changing" or "Changed" or "ThrownExceptions")
                    continue;

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

    public async Task<bool> Save()
    {
        var iniPath = IniFileLocation;
        var backupPath = iniPath + ".bak";

        try
        {
        // Backup the original
        if (File.Exists(iniPath))
        {
            File.Copy(iniPath, backupPath, overwrite: true);
            if (File.Exists(backupPath))
            {
                File.Delete(IniFileLocation);
            }
            else
                return false;
        }

        File.WriteAllText(iniPath, "");
        var ini = new IniFile(iniPath);

        ini.SetValue("RastaConverter", "Location", RastaConverterCommand);
        ini.SetValue("RastaConverter", "HelpFile", HelpFileLocation);
        ini.SetValue("RastaConverter", "PalettesDir", PaletteDirectory);
        ini.SetValue("RastaConverter", "NoNameFilesDir", NoNameFilesLocation);
        ini.SetValue("RastaConverter", "DualModeNoNameFilesDir", DualModeNoNameFilesLocation);

        ini.SetValue("RastaConverter.Defaults", "DefaultUnstuckDrift", RastaConverterDefaultValues.DefaultUnstuckDrift.ToString());
        ini.SetValue("RastaConverter.Defaults", "DefaultUnstuckAfter", RastaConverterDefaultValues.DefaultUnstuckAfter.ToString());
        ini.SetValue("RastaConverter.Defaults", "DefaultPreColourDistance", RastaConverterDefaultValues.DefaultPreColourDistance.ToString());
        ini.SetValue("RastaConverter.Defaults", "DefaultColourDistance", RastaConverterDefaultValues.DefaultColourDistance.ToString());

        ini.SetValue("Tools", "RC2MCH", RC2MCHCommand);
        ini.SetValue("Tools", "MADS", MadsLocation);

        ini.SetValue("Debug", "DebugMode", DebugMode.ToString());
        
        ini.SetValue("UserPreferences", "AutoViewPreview", AutoViewPreview.ToString());        
        ini.SetValue("UserPreferences", "DefaultImageDestination", DefaultImageDestination);

        ini.SetValue("Experimental", "SetConversionIcon", SetConversionIcon.ToString());
        ini.SetValue("Experimental", "RCEditCommand", RCEditCommand);
        ini.SetValue("Experimental", "DryRunDelete", DryRunDelete.ToString());

        
        // Save to temp file
        var iniDIR = Path.GetDirectoryName(IniFileLocation);
        var tempPath = Path.Combine(iniDIR,"settings_raw.ini");
        ini.SaveTo(tempPath);

        // Read lines and inject spacing
        var lines = File.ReadAllLines(tempPath);
        var spacedLines = new List<string>();
        string? lastSection = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                if (lastSection != null) spacedLines.Add(""); // insert blank line before new section
                lastSection = line;
            }

            spacedLines.Add(line);
        }

        File.WriteAllLines(IniFileLocation, spacedLines);
        File.Delete(tempPath);
        
        return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}