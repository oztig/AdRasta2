using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AdRasta2.Enums;
using CliWrap;

namespace AdRasta2.Models;

public class Atari800
{
    public static async Task<AdRastaStatus> RunExecutableAsync(RastaConversion conversion)
    {
        var ret = AdRastaStatus.Success;
        
        var fullExePath = Path.Combine(conversion.DestinationDirectory,conversion.ExecutableFileName + ".xex");
        var toViewParams = new List<string>();
        toViewParams.Add(fullExePath);

        try
        {
            // View the output in Atari Emulator
            var result = await Cli.Wrap(Settings.Current.DefaultExecuteCommand)
                .WithArguments(toViewParams, true)
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync();
            
            ret = AdRastaStatus.Success;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ret = AdRastaStatus.UnknownError;
        }

        return ret;
    }
}