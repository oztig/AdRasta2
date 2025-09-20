using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CliWrap;

namespace AdRasta2.Models;

public class Atari800
{
    public static async Task RunExecutableAsync(RastaConversion conversion)
    {
        var fullExePath = Path.Combine(conversion.DestinationDirectory,conversion.ExecutableFileName + ".xex");
        var toViewParams = new List<string>();
        toViewParams.Add(fullExePath);

        // View the output in Atari Emulator
        var result = await Cli.Wrap(Settings.DefaultExecuteCommand)
            .WithArguments(toViewParams, true)
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
        
    }
}