using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Models;
using AdRasta2.Utils;

namespace AdRasta2.Services;

public class IconPatchService
{
    private readonly IconGlyphService _iconGlyphService;

    public IconPatchService(IconGlyphService iconGlyphService)
    {
        _iconGlyphService = iconGlyphService;
    }

    public async Task GenerateAndPatchAsync(
        string sourceImagePath,
        string destinationPath,
        string overlayText,
        RastaConversion conversion)
    {
        if (!Settings.Current.SetConversionIcon)
            return;

        // Step 1: Generate .bmp and .ico
        await _iconGlyphService.GenerateIconAsync(sourceImagePath, destinationPath, overlayText);

        // Step 2: Patch executable with rcedit (Windows only)
        if (Settings.IsWindows)
        {
            var icoPath = Path.Combine(destinationPath, "RastaConverter.ico");
            var exeToPatch = Path.Combine(destinationPath,conversion.RastaConverterFileName);

            if (File.Exists(icoPath) && File.Exists(exeToPatch) && File.Exists(Settings.Current.RCEditCommand))
            {
                var workingDirectory = destinationPath;
                var rceditExeName = Path.GetFileName(Settings.Current.RCEditCommand);

                var arguments = new List<string>
                {
                    $"\"{exeToPatch}\"",
                    "--set-icon",
                    $"\"{icoPath}\""
                };

                ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                    $"Invoking rcedit:\nExecutable: {rceditExeName}\nWorkingDir: {workingDirectory}\nArgs: {string.Join(" ", arguments)}");

                await ProcessRunner.RunAsync(rceditExeName, workingDirectory, arguments, conversion);
            }
            else
            {
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.Error,
                    $"Missing required files for patching.\nICO: {icoPath}\nEXE: {exeToPatch}\nrcedit: {Settings.Current.RCEditCommand}");
            }
        }
    }
}