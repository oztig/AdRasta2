using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Utils;
using Avalonia.Media.TextFormatting;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;

namespace AdRasta2.Models;

public class RastaConverter
{
    public static async Task<bool> ExecuteCommand(bool isPreview, RastaConversion conversion)
    {
        var ret = false;
        var rastaCommand = Path.Combine(conversion.DestinationDirectory, Settings.BaseRastaCommand);
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var safeParams = await GenerateRastaArguments(isPreview, conversion);

        try
        {
            // Copy supporting files
            await FileUtils.CopyMatchingFilesAsync(Settings.BaseRastaCommandLocation, conversion.DestinationDirectory,
                Settings.BaseRastaCommand);

            // Font File
            await FileUtils.CopyMatchingFilesAsync(Settings.BaseRastaCommandLocation, conversion.DestinationDirectory,
                "clacon2.ttf");

            // Palette Dir
            await FileUtils.CopyDirectoryIncludingRoot(Settings.PaletteDirectory, conversion.DestinationDirectory);

            var cmd = Cli.Wrap(rastaCommand)
                .WithWorkingDirectory(conversion.DestinationDirectory)
                .WithArguments(safeParams, true)
                .WithValidation(CommandResultValidation.None);

            await foreach (var cmdEvent in cmd.ListenAsync())
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        conversion.ProcessID = started.ProcessId;
                        break;

                    case ExitedCommandEvent exited:
                        Console.WriteLine($"Process exited; Code: {exited.ExitCode}");
                        break;
                }
            }

            ret = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ret = false;
        }

        return ret;
    }

    public async static Task<IReadOnlyList<string>> GenerateRastaArguments(bool isPreview, RastaConversion conversion)
    {
        IReadOnlyList<string> args = new List<string>();

        switch (Settings.RastaConverterVersion)
        {
            case < 16:
                args = await GenerateLegacyArguments(isPreview, conversion);
                break;
            case >= 17:
                args = await GenerateNewRastaArguments(isPreview, conversion);
                break;
        }

        return args;
    }

    private async static Task<IReadOnlyList<string>> GenerateLegacyArguments(bool isPreview,
        RastaConversion rastaConversion)
    {
        var args = new List<string>();

        if (isPreview)
            args.Add("/preprocess");

        args.Add($"/i={rastaConversion.SourceImagePath}");

        return args;
    }

    private async static Task<IReadOnlyList<string>> GenerateNewRastaArguments(bool isPreview,
        RastaConversion rastaConversion)
    {
        var args = new List<string>();

        if (isPreview)
            args.Add("--preprocess");
        
        args.Add($"--input={rastaConversion.SourceImagePath}");

        return args;
    }

    // if (!AutoHeight)
    //     args.Add($"/h={Height}");
    //
    // if (SelectedResizeFilter != defaultValues.defaultSelectedResizeFilter)
    //     args.Add($"/filter={SelectedResizeFilter}");
    //
    // if (_selectedPreColourDistance != defaultValues.defaultSelectedPreColourDistance)
    //     args.Add($"/predistance={SelectedPreColourDistance}");
    //
    // if (_selectedDithering != defaultValues.defaultSelectedDithering)
    //     args.Add($"/dither={SelectedDithering}");
    //
    // if (SelectedDithering != "none")
    // {
    //     if (DitheringStrength != defaultValues.defaultDitheringStrength)
    //         args.Add($"/dither_val={DitheringStrength}");
    //
    //     if (DitheringRandomness != defaultValues.defaultDitheringRandomness)
    //         args.Add($"/dither_rand={DitheringRandomness}");
    // }
    //
    // if (Brightness != defaultValues.defaultBrightness)
    //     args.Add($"/brightness={Brightness}");
    //
    // if (Contrast != defaultValues.defaultContrast)
    //     args.Add($"/contrast={Contrast}");
    //
    // if (Gamma != defaultValues.defaultGamma)
    //     args.Add($"/gamma={Gamma}");
    //
    // if (!string.IsNullOrWhiteSpace(MaskFilePath))
    // {
    //     args.Add($"/details={MaskFilePath}");
    //
    //     if (MaskStrength != defaultValues.defaultMaskStrength)
    //         args.Add($"/details_val={MaskStrength}");
    // }
    //
    // if (!string.IsNullOrWhiteSpace(RegisterOnOffFilePath))
    //     args.Add($"/onoff={RegisterOnOffFilePath}");
    //
    // if (SelectedColourDistance != defaultValues.defaultSelectedColourDistance)
    //     args.Add($"/distance={SelectedColourDistance}");
    //
    // if (SelectedInitialState != defaultValues.defaultSelectedInitialState)
    //     args.Add($"/init={SelectedInitialState}");
    //
    // if (NumberOfSolutions != defaultValues.defaultNumberOfSolutions)
    //     args.Add($"/s={NumberOfSolutions}");
    //
    // if (SelectedAutoSavePeriod != defaultValues.defaultSelectedAutoSavePeriod)
    //     args.Add($"/save={SelectedAutoSavePeriod}");
    //
    // args.Add($"/threads={SelectedThread}");
    //
    // if (isPreview)
    //     args.Add("/preprocess");
    //
    // if (isContinue)
    //     args.Add("/continue");
    //
    // args.Add($"/i={SourceFilePath}");
    // args.Add($"/o={FullDestinationFileName}");
    // args.Add($"/pal={Path.Combine(_settings.PaletteDirectory, SelectedPalette.Trim() + ".act")}");
    //
    // RastConverterFullCommandLine = await RastaConverter.GenerateFullCommandLineString(_settings, args);

    // return args;
    // }
}