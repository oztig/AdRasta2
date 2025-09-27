using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Models;
using CliWrap;
using CliWrap.EventStream;

namespace AdRasta2.Utils;

public static class ProcessRunner
{
    
    // var cmd = Cli.Wrap(Settings.BaseRastaCommand)
    //     .WithWorkingDirectory(conversion.DestinationDirectory)
    //     .WithArguments(safeParams, true)
    //     .WithValidation(CommandResultValidation.None);
    
    public static Task <RastaConversion> RunAsync(
        string executablePath,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        RastaConversion conversion)
    {
        var cmd = Cli.Wrap(executablePath)
            .WithArguments(arguments,true)
            .WithWorkingDirectory(workingDirectory)
            .WithValidation(CommandResultValidation.None);

        return Task.Run(async () =>
        {
            try
            {
                await foreach (var cmdEvent in cmd.ListenAsync())
                {
                    switch (cmdEvent)
                    {
                        case StartedCommandEvent started:
                            conversion.ProcessID = started.ProcessId;
                            // ConversionLogger.Log(conversion, ConversionStatus.Running,
                            //     $"Started process {started.ProcessId}");
                            break;

                        case ExitedCommandEvent exited:
                            // ConversionLogger.Log(conversion, ConversionStatus.Completed,
                            //     $"Process {conversion.ProcessID} exited with code {exited.ExitCode}");
                            conversion.ProcessID = 0;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                var trace = new StackTrace();
                var callerFrame = trace.GetFrame(1); // 0 = this method, 1 = caller
                var callerMethod = callerFrame?.GetMethod();
                var callerName = callerMethod != null
                    ? $"{callerMethod.DeclaringType?.FullName}.{callerMethod.Name}"
                    : "UnknownCaller";

                ConversionLogger.Log(conversion, ConversionStatus.Error,
                    $"{executablePath} (called from {callerName})", e);

                conversion.ProcessID = 0;
            }

            return conversion;

        });
    }
}