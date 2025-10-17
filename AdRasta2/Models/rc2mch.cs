using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Utils;
using CliWrap;

namespace AdRasta2.Models;

public class rc2mch
{
    public static async Task<AdRastaStatus> GenerateMCH(RastaConversion conversion)
    {
        var args = new List<string>();
        var mchFile = conversion.DualFrameMode
            ? Path.Combine(conversion.DestinationDirectory, RastaConverterDefaultValues.DefaultDualModeConvertedImageName)
            : Path.Combine(conversion.DestinationDirectory, RastaConverterDefaultValues.DefaultConvertedImageName);

        try
        {
            if (!File.Exists(Settings.Current.RC2MCHCommand))
                return AdRastaStatus.rc2mchNotFound;

            args.Add(SafeCommand.QuoteIfNeeded(mchFile));

            var result = await ProcessRunner.RunAsync(
                Settings.Current.RC2MCHCommand,
                conversion.DestinationDirectory,
                args,
                conversion);

            if (result.Status != AdRastaStatus.Success || result.ExitCode != 0)
            {
                if (Settings.Current.DebugMode)
                {
                    ConversionLogger.Log(conversion, ConversionStatus.Error,
                        $"rc2mch failed with status {result.Status} and exit code {result.ExitCode}");
                }

                return result.Status;
            }

            ConversionLogger.Log(conversion,ConversionStatus.MCHGenerated,"");
            
            return AdRastaStatus.Success;
        }
        catch (Exception ex)
        {
            if (Settings.Current.DebugMode)
            {
                ConversionLogger.Log(conversion, ConversionStatus.Error, "rc2mch.GenerateMCH", ex);
            }

            return AdRastaStatus.UnknownError;
        }
    }

    // public static async Task<AdRastaStatus> GenerateMCH(RastaConversion conversion)
    // {
    //     var args = new List<string>();
    //     var MCHfile = string.Empty;
    //
    //     if (conversion.DualFrameMode)
    //         MCHfile = Path.Combine(conversion.DestinationDirectory,
    //             RastaConverterDefaultValues.DefaultDualModeConvertedImageName);
    //     else
    //         MCHfile = Path.Combine(conversion.DestinationDirectory,
    //             RastaConverterDefaultValues.DefaultConvertedImageName);
    //
    //     try
    //     {
    //         if (!File.Exists(Settings.Current.RC2MCHCommand))
    //             return AdRastaStatus.rc2mchNotFound;
    //
    //         args.Add(SafeCommand.QuoteIfNeeded(MCHfile));
    //
    //         // Run it and return the completed conversion
    //         var toUpdate = await ProcessRunner.RunAsync(
    //             Settings.Current.RC2MCHCommand,
    //             conversion.DestinationDirectory,
    //             args,
    //             conversion);
    //         
    //         toUpdate.Statuses.AddEntry(DateTime.Now, ConversionStatus.MCHGenerated,
    //             "");
    //
    //         return AdRastaStatus.Success;
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine(ex);
    //         return AdRastaStatus.UnknownError;
    //     }
    // }
}