using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Utils;
using CliWrap;
using DynamicData;

namespace AdRasta2.Models;

public class Mads
{
    private static string _copyFromDir = String.Empty;
    private static string _copyToDir = string.Empty;

    public new List<string> MadsCommandLineArguments { get; set; } = new List<string>();

    public static async Task<AdRastaStatus> GenerateExecutableFileAsync(RastaConversion conversion)
    {
        var ret = AdRastaStatus.UnknownError;

        _copyFromDir = conversion.DualFrameMode
            ? Settings.DualModeNoNameFilesLocation
            : Settings.NoNameFilesLocation;
        _copyToDir = conversion.DestinationDirectory;

        try
        {
            if ((ret = await CopyBuildFilesAsync(conversion)) != AdRastaStatus.Success)
                return ret;

            ret = await GenerateXexAsync(conversion);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ret = AdRastaStatus.UnknownError;
        }

        return ret;
    }

    private static async Task<AdRastaStatus> GenerateXexAsync(RastaConversion conversion)
    {
        var madsCommandLineArguments = new List<string>();
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var asqLocation = Path.Combine(_copyToDir, Settings.NoNameAsq);

        if (!File.Exists(Settings.MadsLocation))
            return AdRastaStatus.MADSNotFound;

        madsCommandLineArguments.Add(SafeCommand.QuoteIfNeeded(asqLocation));
        madsCommandLineArguments.Add(" -o:" +
                                     SafeCommand.QuoteIfNeeded(Path.Combine(conversion.DestinationDirectory,
                                         conversion.ExecutableFileName.Trim() + ".xex")));

        // Now Run Mads to Generate the output file
        await Cli.Wrap(Settings.MadsLocation)
            .WithArguments(madsCommandLineArguments, false)
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
            .ExecuteAsync();

        return AdRastaStatus.Success;
    }

    public static async Task<AdRastaStatus> CopyBuildFilesAsync(RastaConversion conversion)
    {
        var filesToCopy = new[]
        {
            (Source: Path.Combine(_copyFromDir, Settings.NoNameAsq),
                Destination: Path.Combine(_copyToDir, Settings.NoNameAsq)),
            (Source: Path.Combine(_copyFromDir, Settings.NoNameHeader),
                Destination: Path.Combine(_copyToDir, Settings.NoNameHeader))
        };

        try
        {
            if (!Directory.Exists(_copyToDir))
                return AdRastaStatus.DestinationDirectoryMissing;

            foreach (var (source, destination) in filesToCopy)
            {
                if (!File.Exists(source))
                    return AdRastaStatus.SourceFileMissing;

                File.Copy(source, destination, overwrite: true);
            }

            return AdRastaStatus.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CopyBuildFilesAsync] Ritual failed: {ex.Message}");
            return AdRastaStatus.UnknownError;
        }
    }
}