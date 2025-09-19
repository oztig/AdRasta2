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
    public static async Task<bool> ExecuteCommand(bool isPreview,bool isContinue, RastaConversion conversion)
    {
        var ret = false;
        var rastaCommand = Path.Combine(conversion.DestinationDirectory, Settings.BaseRastaCommand);
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var safeParams = await GenerateRastaArguments(isPreview,isContinue, conversion);

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

    public async static Task<IReadOnlyList<string>> GenerateRastaArguments(bool isPreview,bool isContinue, RastaConversion conversion)
    {
        IReadOnlyList<string> args = new List<string>();

        switch (Settings.RastaConverterVersion)
        {
            case < 16:
                args = await GenerateLegacyArguments(isPreview,isContinue, conversion);
                break;
            case >= 16:
                args = await GenerateNewRastaArguments(isPreview,isContinue, conversion);
                break;
        }

        return args;
    }

    private async static Task<IReadOnlyList<string>> GenerateLegacyArguments(bool isPreview,bool isContinue,
        RastaConversion rastaConversion)
    {
        var args = new List<string>();

        if (isContinue)
        {
            args.Add("/continue");
            return args;
        }
        
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

        if (!string.IsNullOrWhiteSpace(rastaConversion.RegisterOnOffFilePath))
            args.Add($"/onoff={rastaConversion.RegisterOnOffFilePath}");

        if (rastaConversion.ColourDistance != RastaConverterDefaultValues.DefaultColourDistance)
            args.Add($"/distance={rastaConversion.ColourDistance}");
        
        if (rastaConversion.InitialState != RastaConverterDefaultValues.DefaultInitialState)
            args.Add($"/init={rastaConversion.InitialState }");

        if (rastaConversion.SolutionHistoryLength != RastaConverterDefaultValues.DefaultSolutionHistoryLength)
            args.Add($"/s={rastaConversion.SolutionHistoryLength}");

        if (rastaConversion.AutoSavePeriod != RastaConverterDefaultValues.DefaultAutoSavePeriod)
            args.Add($"/save={rastaConversion.AutoSavePeriod}");

        args.Add($"/threads={rastaConversion.NumberOfThreads}");
        
        if (rastaConversion.Palette != RastaConverterDefaultValues.DefaultPalette)
        {
            var paletteFile = Path.Combine("Palettes", rastaConversion.Palette.ToString().ToLower().Trim() + ".act");
            args.Add($"/pal={paletteFile}");
        }

        
        
        args.Add($"/i={rastaConversion.SourceImagePath}");

        return args;
    }

    private async static Task<IReadOnlyList<string>> GenerateNewRastaArguments(bool isPreview,bool isContinue,
        RastaConversion rastaConversion)
    {
        var args = new List<string>();

        if (isContinue)
        {
            args.Add("--continue");
            return args;
        }
            
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
        
        if (!string.IsNullOrWhiteSpace(rastaConversion.RegisterOnOffFilePath))
            args.Add($"--onoff={rastaConversion.RegisterOnOffFilePath}");

        if (rastaConversion.ColourDistance != RastaConverterDefaultValues.DefaultColourDistance)
            args.Add($"--distance={rastaConversion.ColourDistance}");
        
        if (rastaConversion.InitialState != RastaConverterDefaultValues.DefaultInitialState)
            args.Add($"--init={rastaConversion.InitialState }");
        
        if (rastaConversion.SolutionHistoryLength != RastaConverterDefaultValues.DefaultSolutionHistoryLength)
            args.Add($"--solutions={rastaConversion.SolutionHistoryLength}");
        
        if (rastaConversion.AutoSavePeriod != RastaConverterDefaultValues.DefaultAutoSavePeriod)
            args.Add($"--save={rastaConversion.AutoSavePeriod}");
        
        args.Add($"--threads={rastaConversion.NumberOfThreads}");
        
        if (rastaConversion.Palette != RastaConverterDefaultValues.DefaultPalette)
        {
            var paletteFile = Path.Combine("Palettes", rastaConversion.Palette.ToString().ToLower().Trim() + ".act");
            args.Add($"-pal={paletteFile}");
        }
        
        
        args.Add($"--input={rastaConversion.SourceImagePath}");


        return args;
    }

    

    //
    // args.Add($"/i={SourceFilePath}");
    // args.Add($"/o={FullDestinationFileName}");
    //
}