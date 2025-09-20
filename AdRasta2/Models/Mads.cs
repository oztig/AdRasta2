using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdRasta2.Utils;
using CliWrap;
using DynamicData;

namespace AdRasta2.Models;

public class Mads
{
    private static string _copyFromDir = String.Empty;
    private static string _copyToDir = string.Empty;

    public new List<string> MadsCommandLineArguments { get; set; } = new List<string>();

    public static async Task GenerateExecutableFileAsync(RastaConversion conversion)
    {
        _copyFromDir = conversion.DualFrameMode
            ? Settings.DualModeNoNameFilesLocation
            : Settings.NoNameFilesLocation;
        _copyToDir = conversion.DestinationDirectory;

        try
        {
            if (await CopyBuildFilesAsync(conversion))
            {
                // Do something !
                await GenerateXexAsync(conversion);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static async Task<bool> GenerateXexAsync(RastaConversion conversion)
    {
        var madsCommandLineArguments = new List<string>();
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var asqLocation = Path.Combine(_copyToDir, Settings.NoNameAsq);

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

        return true;
    }

    public static async Task<bool> CopyBuildFilesAsync(RastaConversion conversion)
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
                return false;

            foreach (var (source, destination) in filesToCopy)
            {
                if (!File.Exists(source))
                    return false;

                File.Copy(source, destination, overwrite: true);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CopyBuildFilesAsync] Ritual failed: {ex.Message}");
            return false;
        }
    }
}