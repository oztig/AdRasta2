using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Utils;
using CliWrap;

namespace AdRasta2.Models;

public class rc2mch
{
    public static async Task<AdRastaStatus> GenerateMCH(string rc2MCHExecutable, string sourceFile)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        try
        {

            if (!File.Exists(rc2MCHExecutable))
                return AdRastaStatus.rc2mchNotFound;
                
            await Cli.Wrap(rc2MCHExecutable)
                .WithArguments(SafeCommand.QuoteIfNeeded(sourceFile))
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
                .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
                .ExecuteAsync();
            
            return AdRastaStatus.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return AdRastaStatus.UnknownError;
        }
    }
}