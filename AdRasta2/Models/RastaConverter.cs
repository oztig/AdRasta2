using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Services;
using AdRasta2.Utils;
using Avalonia.Media.TextFormatting;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;

namespace AdRasta2.Models;

public class RastaConverter
{
    private static async Task CopySupportingFiles(RastaConversion conversion)
    {
        // Copy supporting files
        FileUtils.CopyFile(Settings.Current.RastaConverterCommand,
            Path.Combine(conversion.DestinationFilePath, conversion.RastaConverterFileName), true);

        await FileUtils.CopyMatchingFilesAsync(Settings.Current.BaseRastaCommandLocation,
            conversion.DestinationDirectory,
            "*.dll", conversion, false);

        await FileUtils.CopyMatchingFilesAsync(Settings.Current.BaseRastaCommandLocation,
            conversion.DestinationDirectory,
            "clacon2.ttf", conversion, false);

        await FileUtils.CopyMatchingFilesAsync(Settings.Current.BaseRCEditCommandLocation,
            conversion.DestinationDirectory,
            Settings.Current.BaseRCEditCommand, conversion, false);

        await FileUtils.CopyDirectoryIncludingRoot(Settings.Current.PaletteDirectory, conversion.DestinationDirectory);
    }

    public static async Task<ProcessRunResult> ExecuteCommand(bool isPreview, bool isContinue,
        RastaConversion conversion, IconPatchService iconService)
    {
        var safeParams = await GenerateRastaArguments(isPreview, isContinue, conversion);
        conversion.CommandLineText = await GenerateFullCommandLineString(safeParams);

        try
        {
            await CopySupportingFiles(conversion);

            // Generate the image for the icon
            await iconService.GenerateAndPatchAsync(conversion.SourceImagePath, conversion.DestinationFilePath,
                conversion.Title, conversion);

            // Run it and return the completed conversion
            return await ProcessRunner.RunAsync(
                conversion.RastaConverterFileName,
                conversion.DestinationDirectory,
                safeParams,
                conversion);
        }
        catch (Exception e)
        {
            ConversionLogger.Log(conversion, ConversionStatus.Error, "RastaConverter.ExecuteCommand", e);
            conversion.ProcessID = 0;

            return new ProcessRunResult
            {
                Conversion = conversion,
                Status = AdRastaStatus.UnknownError,
                ExitCode = -1,
                StandardOutput = null,
                StandardError = e.ToString()
            };
        }
    }

    public async static Task<IReadOnlyList<string>> GenerateRastaArguments(bool isPreview, bool isContinue,
        RastaConversion conversion)
    {
        IReadOnlyList<string> args = new List<string>();

        switch (Settings.Current.RastaConverterVersion)
        {
            case < 16:
                args = await GenerateLegacyArguments(isPreview, isContinue, conversion);
                break;
            case >= 16:
                args = await GenerateNewRastaArguments(isPreview, isContinue, conversion);
                break;
        }

        return args;
    }

    private async static Task<IReadOnlyList<string>> GenerateLegacyArguments(bool isPreview, bool isContinue,
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

        if (!string.IsNullOrWhiteSpace(rastaConversion.RegisterOnOffFilePath))
            args.Add($"/onoff={rastaConversion.RegisterOnOffFilePath}");

        if (rastaConversion.ColourDistance != RastaConverterDefaultValues.DefaultColourDistance)
            args.Add($"/distance={rastaConversion.ColourDistance}");

        if (rastaConversion.InitialState != RastaConverterDefaultValues.DefaultInitialState)
            args.Add($"/init={rastaConversion.InitialState}");

        if (rastaConversion.SolutionHistoryLength != RastaConverterDefaultValues.DefaultSolutionHistoryLength)
            args.Add($"/s={rastaConversion.SolutionHistoryLength}");

        if (rastaConversion.AutoSavePeriod != RastaConverterDefaultValues.DefaultAutoSavePeriod)
            args.Add($"/save={rastaConversion.AutoSavePeriod}");

        args.Add($"/threads={rastaConversion.NumberOfThreads}");

        if (rastaConversion.Palette != RastaConverterDefaultValues.DefaultPalette)
        {
            var paletteFile = Path.Combine("Palettes", rastaConversion.Palette.Trim() + ".act");
            args.Add($"/pal={paletteFile}");
        }

        if (rastaConversion.MaxEvaluations != RastaConverterDefaultValues.DefaultMaxEvaluations)
            args.Add($"/me={rastaConversion.MaxEvaluations}");

        if (rastaConversion.RandomSeed != RastaConverterDefaultValues.DefaultRandomSeed)
            args.Add($"/seed={rastaConversion.RandomSeed}");

        if (rastaConversion.Optimiser != RastaConverterDefaultValues.DefaultOptimiser)
        {
            args.Add($"/opt={rastaConversion.Optimiser}");
        }

        if (rastaConversion.CacheInMB != RastaConverterDefaultValues.DefaultCacheInMB)
        {
            args.Add($"/cache={rastaConversion.CacheInMB}");
        }

        if (rastaConversion.DualFrameMode)
        {
            args.Add($"/dual");
        }

        if (rastaConversion.FirstDualSteps != RastaConverterDefaultValues.DefualtFirstDualSteps)
        {
            args.Add($"/fds={rastaConversion.FirstDualSteps}");
        }

        if (rastaConversion.AfterDualSteps != RastaConverterDefaultValues.DefaultAfterDualSteps)
        {
            args.Add($"/ads={rastaConversion.AfterDualSteps}");
        }

        if (rastaConversion.AlternatingDualSteps != RastaConverterDefaultValues.DefaultAlternatingDualSteps)
        {
            args.Add($"/alts={rastaConversion.AlternatingDualSteps}");
        }

        if (rastaConversion.UnstuckAfter != RastaConverterDefaultValues.DefaultUnstuckAfter)
        {
            args.Add($"/unstuck_after={rastaConversion.UnstuckAfter}");
        }

        if (rastaConversion.UnstuckDrift != RastaConverterDefaultValues.DefaultUnstuckDrift)
        {
            args.Add($"/unstuck_drift={rastaConversion.UnstuckDrift}");
        }

        if (rastaConversion.DualBlending != RastaConverterDefaultValues.DefaultDualBlending)
        {
            args.Add($"/db={rastaConversion.DualBlending}");
        }

        if (rastaConversion.DualLuma != RastaConverterDefaultValues.DefaultDualLuma)
        {
            args.Add($"/dl={rastaConversion.DualLuma}");
        }

        if (rastaConversion.DualChroma != RastaConverterDefaultValues.DefaultDualChroma)
        {
            args.Add($"/dc={rastaConversion.DualChroma}");
        }

        args.Add($"/i={rastaConversion.SourceImageBaseName}");

        if (!string.IsNullOrWhiteSpace(rastaConversion.SourceImageMaskBaseName))
        {
            args.Add($"/details={rastaConversion.SourceImageMaskPath}");

            if (rastaConversion.MaskStrength != RastaConverterDefaultValues.DefaultMaskStrength)
                args.Add($"/details_val={rastaConversion.MaskStrength}");
        }

        return args;
    }

    private async static Task<IReadOnlyList<string>> GenerateNewRastaArguments(bool isPreview, bool isContinue,
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

        if (!string.IsNullOrWhiteSpace(rastaConversion.RegisterOnOffFilePath))
            args.Add($"--onoff={rastaConversion.RegisterOnOffFilePath}");

        if (rastaConversion.ColourDistance != RastaConverterDefaultValues.DefaultColourDistance)
            args.Add($"--distance={rastaConversion.ColourDistance}");

        if (rastaConversion.InitialState != RastaConverterDefaultValues.DefaultInitialState)
            args.Add($"--init={rastaConversion.InitialState}");

        if (rastaConversion.SolutionHistoryLength != RastaConverterDefaultValues.DefaultSolutionHistoryLength)
            args.Add($"--solutions={rastaConversion.SolutionHistoryLength}");

        if (rastaConversion.AutoSavePeriod != RastaConverterDefaultValues.DefaultAutoSavePeriod)
            args.Add($"--save={rastaConversion.AutoSavePeriod}");

        args.Add($"--threads={rastaConversion.NumberOfThreads}");

        if (rastaConversion.Palette != RastaConverterDefaultValues.DefaultPalette)
        {
            var paletteFile = Path.Combine("Palettes", rastaConversion.Palette.Trim() + ".act");
            args.Add($"-pal={paletteFile}");
        }

        if (rastaConversion.MaxEvaluations != RastaConverterDefaultValues.DefaultMaxEvaluations)
            args.Add($"--max_evals={rastaConversion.MaxEvaluations}");

        if (rastaConversion.RandomSeed != RastaConverterDefaultValues.DefaultRandomSeed)
            args.Add($"--seed={rastaConversion.RandomSeed}");

        if (rastaConversion.Optimiser != RastaConverterDefaultValues.DefaultOptimiser)
        {
            args.Add($"--opt={rastaConversion.Optimiser}");
        }

        if (rastaConversion.CacheInMB != RastaConverterDefaultValues.DefaultCacheInMB)
        {
            args.Add($"--cache={rastaConversion.CacheInMB}");
        }

        if (rastaConversion.DualFrameMode)
        {
            args.Add($"--dual");
        }

        if (rastaConversion.FirstDualSteps != RastaConverterDefaultValues.DefualtFirstDualSteps)
        {
            args.Add($"--first_dual_steps={rastaConversion.FirstDualSteps}");
        }

        if (rastaConversion.AfterDualSteps != RastaConverterDefaultValues.DefaultAfterDualSteps)
        {
            args.Add($"--after_dual_steps={rastaConversion.AfterDualSteps}");
        }

        if (rastaConversion.AlternatingDualSteps != RastaConverterDefaultValues.DefaultAlternatingDualSteps)
        {
            args.Add($"--altering_dual_steps={rastaConversion.AlternatingDualSteps}");
        }

        if (rastaConversion.UnstuckAfter != RastaConverterDefaultValues.DefaultUnstuckAfter)
        {
            args.Add($"--unstuck_after={rastaConversion.UnstuckAfter}");
        }

        if (rastaConversion.UnstuckDrift != RastaConverterDefaultValues.DefaultUnstuckDrift)
        {
            args.Add($"--unstuck_drift={rastaConversion.UnstuckDrift}");
        }

        if (rastaConversion.DualBlending != RastaConverterDefaultValues.DefaultDualBlending)
        {
            args.Add($"--dual_blending={rastaConversion.DualBlending}");
        }

        if (rastaConversion.DualLuma != RastaConverterDefaultValues.DefaultDualLuma)
        {
            args.Add($"--dual_luma={rastaConversion.DualLuma}");
        }

        if (rastaConversion.DualChroma != RastaConverterDefaultValues.DefaultDualChroma)
        {
            args.Add($"--dual_chroma={rastaConversion.DualChroma}");
        }

        args.Add($"--input={SafeCommand.QuoteIfNeeded(rastaConversion.SourceImageBaseName)}");

        if (!string.IsNullOrWhiteSpace(rastaConversion.SourceImageMaskBaseName))
        {
            args.Add($"--details={SafeCommand.QuoteIfNeeded(rastaConversion.SourceImageMaskBaseName)}");

            if (rastaConversion.MaskStrength != RastaConverterDefaultValues.DefaultMaskStrength)
                args.Add($"--details_val={rastaConversion.MaskStrength}");
        }

        return args;
    }

    public async static Task<string> GenerateFullCommandLineString(IReadOnlyList<string> argsString)
    {
        var fullCommandLine = $"{Settings.Current.BaseRastaCommand} {string.Join(" ", argsString)}";
        return fullCommandLine;
    }
}