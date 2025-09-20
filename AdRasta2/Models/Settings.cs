using System;
using System.IO;

namespace AdRasta2.Models;

public class Settings
{
    public static string IniFileLocation = Path.Combine(Directory.GetCurrentDirectory().Trim(), "AdRasta2.ini");
    public static string RastaConverterCommand { get; set; } = string.Empty;

    public static string BaseRastaCommandLocation => Path.GetDirectoryName(RastaConverterCommand);
    public static string BaseRastaCommand => Path.GetFileName(RastaConverterCommand);

    public static string RC2MCHCommand { get; set; } = string.Empty;
    public static string DefaultExecuteCommand { get; set; } = string.Empty;
    public static string PaletteDirectory { get; set; } = string.Empty;
    public static string MadsLocation { get; set; } = string.Empty;
    public static string NoNameFilesLocation { get; set; } = string.Empty;
    public static string DualModeNoNameFilesLocation { get; set; } = string.Empty;

    public static string HelpFileLocation { get; set; } = string.Empty;

    public static bool CopyWithoutConfirm { get; set; }

    public static bool PopulateDefaultFile { get; set; }

    public static double RastaConverterVersion { get; set; }
    public static string NoNameHeader { get; set; } = "no_name.h";
    public static string NoNameAsq { get; set; } = "no_name.asq";

    static Settings()
    {
        SetDefaults();
    }

    public static bool CheckIniFileExists()
    {
        return File.Exists(IniFileLocation);
    }

    public static bool SetDefaults()
    {
        if (OperatingSystem.IsWindows())
            DefaultExecuteCommand = "explorer";
        else if (OperatingSystem.IsLinux())
            DefaultExecuteCommand = "xdg-open";
        else
            DefaultExecuteCommand = "explorer";

        if (!CheckIniFileExists())
            return false;

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
            ini.GetStr("Locations", "MultiModeNoNameFilesDir", string.Empty);
        CopyWithoutConfirm = ini.GetBool("Continue", "CopyWithoutConfirm", false);
        PopulateDefaultFile = ini.GetBool("Continue", "PopulateDefaultFile", false);

        // RastaConverter Specific
        RastaConverterVersion = ini.GetDouble("RastaConverter", "Version", 16);
        RastaConverterCommand = ini.GetStr("RastaConverter", "Location",
            string.Empty); // = "/home/nickp/Downloads/RastaConverter-master/src/rastaconv";

        return true;
    }
}