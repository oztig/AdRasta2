using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            case >= 16:
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
        {
            args.Add("/preprocess");
            args.Add("/q");
        }

        if (!rastaConversion.AutoHeight)
            args.Add($"/h={rastaConversion.Height}");

        if (rastaConversion.ResizeFilter != RastaConverterDefaultValues.DefaultResizeFilter)
            args.Add($"/filter={rastaConversion.ResizeFilter}");

        if (rastaConversion.PreColourDistance != RastaConverterDefaultValues.DefaultPreColourDistance)
            args.Add($"/predistance={rastaConversion.PreColourDistance}");

        if (rastaConversion.Dithering != RastaConverterDefaultValues.DefaultDithering)
            args.Add($"/dither={rastaConversion.Dithering}");

        if (rastaConversion.Dithering != "none")
        {
            if (rastaConversion.DitheringStrength != RastaConverterDefaultValues.DefaultDitheringStrength)
                args.Add($"/dither_val={rastaConversion.DitheringStrength}");

            if (rastaConversion.DitheringRandomness != RastaConverterDefaultValues.DefaultDitheringRandomness)
                args.Add($"/dither_rand={rastaConversion.DitheringRandomness}");
        }

        if (rastaConversion.Brightness != RastaConverterDefaultValues.DefaultBrightness)
            args.Add($"/brightness={rastaConversion.Brightness}");

        if (rastaConversion.Contrast != RastaConverterDefaultValues.DefaultContrast)
            args.Add($"/contrast={rastaConversion.Contrast}");

        if (rastaConversion.Gamma != RastaConverterDefaultValues.DefaultGamma)
            args.Add($"/gamma={rastaConversion.Gamma}");

        if (!string.IsNullOrWhiteSpace(rastaConversion.SourceImageMaskPath))
        {
            args.Add($"/details={rastaConversion.SourceImageMaskPath}");

            if (rastaConversion.MaskStrength != RastaConverterDefaultValues.DefaultMaskStrength)
                args.Add($"/details_val={rastaConversion.MaskStrength}");
        }


        args.Add($"/i={rastaConversion.SourceImagePath}");

        return args;
    }

    private async static Task<IReadOnlyList<string>> GenerateNewRastaArguments(bool isPreview,
        RastaConversion rastaConversion)
    {
        var args = new List<string>();

        if (isPreview)
        {
            args.Add("--preprocess");
            args.Add("-q");
        }

        if (!rastaConversion.AutoHeight)
            args.Add($"-h={rastaConversion.Height}");

        if (rastaConversion.ResizeFilter != RastaConverterDefaultValues.DefaultResizeFilter)
            args.Add($"--filter={rastaConversion.ResizeFilter}");

        if (rastaConversion.PreColourDistance != RastaConverterDefaultValues.DefaultPreColourDistance)
            args.Add($"--predistance={rastaConversion.PreColourDistance}");

        if (rastaConversion.Dithering != RastaConverterDefaultValues.DefaultDithering)
            args.Add($"--dither={rastaConversion.Dithering}");

        if (rastaConversion.Dithering != "none")
        {
            if (rastaConversion.DitheringStrength != RastaConverterDefaultValues.DefaultDitheringStrength)
                args.Add($"--dither_val={rastaConversion.DitheringStrength}");

            if (rastaConversion.DitheringRandomness != RastaConverterDefaultValues.DefaultDitheringRandomness)
                args.Add($"--dither_rand={rastaConversion.DitheringRandomness}");
        }

        if (rastaConversion.Brightness != RastaConverterDefaultValues.DefaultBrightness)
            args.Add($"--brightness={rastaConversion.Brightness}");

        if (rastaConversion.Contrast != RastaConverterDefaultValues.DefaultContrast)
            args.Add($"--contrast={rastaConversion.Contrast}");

        if (rastaConversion.Gamma != RastaConverterDefaultValues.DefaultGamma)
            args.Add($"--gamma={rastaConversion.Gamma}");

        if (!string.IsNullOrWhiteSpace(rastaConversion.SourceImageMaskPath))
        {
            args.Add($"--details={rastaConversion.SourceImageMaskPath}");

            if (rastaConversion.MaskStrength != RastaConverterDefaultValues.DefaultMaskStrength)
                args.Add($"--details_val={rastaConversion.MaskStrength}");
        }

        args.Add($"--input={rastaConversion.SourceImagePath}");


        return args;
    }


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