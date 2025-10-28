using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AdRasta2.Models;
using AdRasta2.Enums;

namespace AdRasta2.Utils;

public static class DesktopFileGenerator
{
    public static async Task<string> GenerateDesktopFileAsync(RastaConversion conversion, IReadOnlyList<string> safeParams)
    {
        string uniqueId = "RC_" + DateTime.Now.ToString("yyyyMMddHHmmss");
        string desktopFileName = $"rasta-{uniqueId}.desktop";
        string desktopDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "share", "applications"
        );
        Directory.CreateDirectory(desktopDir);
        string desktopFilePath = Path.Combine(desktopDir, desktopFileName);

        string fullExecutablePath = Path.Combine(
            conversion.DestinationDirectory,
            conversion.RastaConverterFileName
        );

        string execLine = $"{fullExecutablePath} {string.Join(" ", safeParams)}";

        string desktopContent = $@"
[Desktop Entry]
Name=RastaConverter {uniqueId}
Exec={execLine}
Icon={conversion.IconFilePath}
Type=Application
Terminal=false
";
        
        await File.WriteAllTextAsync(desktopFilePath, desktopContent.Trim());

        ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
            $"Generated .desktop file:\n{desktopFilePath}\nContents:\n{desktopContent}");

        return desktopFilePath;
    }
}