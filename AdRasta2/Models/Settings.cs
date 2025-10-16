using System;
using System.IO;
using AdRasta2.Enums;
using AdRasta2.Utils;
using Sini;

namespace AdRasta2.Models;

public class Settings
{
    public static readonly bool IsWindows = OperatingSystem.IsWindows();

    public static readonly string
        IniFileLocation = Path.Combine(Directory.GetCurrentDirectory().Trim(), "AdRasta2.ini");

    public static string RastaConverterCommand { get; set; } = string.Empty;

    public static string BaseRastaCommandLocation => Path.GetDirectoryName(RastaConverterCommand);
    public static string BaseRastaCommand => Path.GetFileName(RastaConverterCommand);

    public static string RC2MCHCommand { get; set; } = string.Empty;
    public static string DefaultExecuteCommand { get; set; } = string.Empty;
    public static string PaletteDirectory { get; set; } = string.Empty;
    public static string MadsLocation { get; set; } = string.Empty;
    public static string MadsLocationBaseName => Path.GetFileName(MadsLocation);
    public static string NoNameFilesLocation { get; set; } = string.Empty;
    public static string DualModeNoNameFilesLocation { get; set; } = string.Empty;

    public static string HelpFileLocation { get; set; } = string.Empty;

    public static bool CopyWithoutConfirm { get; set; }

    public static bool PopulateDefaultFile { get; set; }

    public static double RastaConverterVersion { get; set; }
    public static string NoNameHeader { get; set; } = "no_name.h";
    public static string NoNameAsq { get; set; } = "no_name.asq";

    private static bool _debugMode = false;

    public static bool DebugMode
    {
        get => _debugMode;
        set
        {
            _debugMode = value;
            MaxLogEntries = value ? 500 : 100;
        }
    }

    public static bool AutoViewPreview { get; set; }
    public static bool SetConversionIcon { get; set; }
    public static string RCEditCommand { get; set; }
    public static string BaseRCEditCommandLocation => Path.GetDirectoryName(RCEditCommand);
    public static string BaseRCEditCommand => Path.GetFileName(RCEditCommand);

    public static int MaxLogEntries { get; set; } = 100;


    static Settings()
    {
        // SetDefaults(loggingConversion);
    }

    public static bool CheckIniFileExists()
    {
        return File.Exists(IniFileLocation);
    }

    public static bool SetDefaults(RastaConversion loggingConversion)
    {
        if (Settings.IsWindows)
            DefaultExecuteCommand = "explorer";
        else if (OperatingSystem.IsLinux())
            DefaultExecuteCommand = "xdg-open";
        else
            DefaultExecuteCommand = "explorer";

        if (!CheckIniFileExists())
            return false;

        try
        {
            var ini = new Sini.IniFile(IniFileLocation);

            // Get from settings file, or ideally allow user to select
            HelpFileLocation =
                ini.GetStr("Locations", "RastaHelpFile",
                    string.Empty); // "/home/nickp/Downloads/RastaConverter-master/help.txt";
            RC2MCHCommand =
                ini.GetStr("Locations", "RC2MCH",
                    string.Empty); // = "/home/nickp/Downloads/RastaConverter-master/src/rastaconv";
            MadsLocation =
                ini.GetStr("Locations", "MADS", string.Empty); // "/home/nickp/WUDSN/Tools/ASM/MADS/mads.linux-x86-64";
            PaletteDirectory =
                ini.GetStr("Locations", "PalettesDir",
                    string.Empty); // "/home/nickp/Downloads/RastaConverter-master/src/Palettes";
            NoNameFilesLocation =
                ini.GetStr("Locations", "NoNameFilesDir",
                    string.Empty); // "/home/nickp/Downloads/RastaConverter-master/Generator";
            DualModeNoNameFilesLocation =
                ini.GetStr("Locations", "DualModeNoNameFilesDir", string.Empty);
            CopyWithoutConfirm = GetSafeBool(ini, "Continue", "CopyWithoutConfirm", false);
            PopulateDefaultFile = GetSafeBool(ini, "Continue", "PopulateDefaultFile", false);
            DebugMode = GetSafeBool(ini, "Debug", "DebugMode", false);
            AutoViewPreview = GetSafeBool(ini, "UserDefaults", "AutoViewPreview", false);

            // RastaConverter Specific
            RastaConverterCommand = ini.GetStr("RastaConverter", "Location",
                "Cant Find in ini File");
            RastaConverterVersion = GetSafeDouble(ini, "RastaConverter", "Version", 17);

            float raw = GetSafeFloat(ini, "RastaConverter.Defaults", "DefaultUnstuckDrift", 0.00001f);
            decimal unstuckDrift = (decimal)raw;
            RastaConverterDefaultValues.DefaultUnstuckDrift = unstuckDrift;
            RastaConverterDefaultValues.DefaultUnstuckAfter =
                ini.GetInt("RastaConverter.Defaults", "DefaultUnstuckAfter", 100000);

            // Experimental
            SetConversionIcon = GetSafeBool(ini, "Experimental", "SetConversionIcon", false);
            RCEditCommand = ini.GetStr("Experimental", "RCEditCommand", string.Empty);
        }
        catch (Exception ex)
        {
            ConversionLogger.LogIfDebug(loggingConversion, ConversionStatus.Debug,
                "Error Reading Settings", ex, "", true);
        }

        return true;
    }

    public static double GetSafeDouble(IniFile ini, string section, string key, double fallback)
    {
        var str = ini.GetStr(section, key);
        return double.TryParse(str, out var result) ? result : fallback;
    }

    public static bool GetSafeBool(IniFile ini, string section, string key, bool fallback)
    {
        var str = ini.GetStr(section, key);
        return bool.TryParse(str, out var result) ? result : fallback;
    }

    public static float GetSafeFloat(IniFile ini, string section, string key, float fallback)
    {
        var str = ini.GetStr(section, key);
        return float.TryParse(str, out var result) ? result : fallback;
    }

    public static void LogSettingValues(RastaConversion loggingConversion)
    {
        var props = typeof(Settings).GetProperties(System.Reflection.BindingFlags.Public |
                                                   System.Reflection.BindingFlags.Static);

        foreach (var prop in props)
        {
            try
            {
                var value = prop.GetValue(null);
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