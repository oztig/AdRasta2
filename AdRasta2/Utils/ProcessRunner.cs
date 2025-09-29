using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AdRasta2.Enums;
using AdRasta2.Models;
using CliWrap;
using CliWrap.EventStream;

namespace AdRasta2.Utils;

public static class ProcessRunner
{
    public static async Task<ProcessRunResult> RunAsync(
        string executablePath,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        RastaConversion conversion)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();
        var trace = new StackTrace();
        var callerFrame = trace.GetFrame(1);
        var callerMethod = callerFrame?.GetMethod();
        var callerName = callerMethod != null
            ? $"{callerMethod.DeclaringType?.FullName}.{callerMethod.Name}"
            : "UnknownCaller";

        int exitCode = -1;

        try
        {
            var fullExecutablePath = Path.Combine(workingDirectory, executablePath);
            if (!File.Exists(fullExecutablePath))
            {
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.Error,
                    $"Executable not found at '{fullExecutablePath}'");
                return new ProcessRunResult
                {
                    Conversion = conversion,
                    Status = AdRastaStatus.UnknownError,
                    ExitCode = -1,
                    StandardOutput = null,
                    StandardError = "Executable not found."
                };
            }
            
            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Debug,
                $"Resolved executable path: {fullExecutablePath}\nWorking directory: {workingDirectory}");


            var cmd = Cli.Wrap(fullExecutablePath)
                .WithArguments(arguments, false)
                .WithWorkingDirectory(workingDirectory)
                .WithValidation(CommandResultValidation.None);

            await foreach (var cmdEvent in cmd.ListenAsync())
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        conversion.ProcessID = started.ProcessId;
                        ConversionLogger.LogIfDebug(conversion, ConversionStatus.Running,
                            $"Started process {started.ProcessId} — {Path.GetFileName(executablePath)} (from {callerName})");
                        break;

                    case StandardOutputCommandEvent stdOut:
                        stdOutBuffer.AppendLine(stdOut.Text);
                        break;

                    case StandardErrorCommandEvent stdErr:
                        stdErrBuffer.AppendLine(stdErr.Text);
                        break;

                    case ExitedCommandEvent exited:
                        conversion.ProcessID = 0;
                        exitCode = exited.ExitCode;
                        ConversionLogger.LogIfDebug(conversion, ConversionStatus.Completed,
                            $"Process exited with code {exitCode} — {Path.GetFileName(executablePath)} (from {callerName})");
                        break;
                }
            }

            if (stdOutBuffer.Length > 0)
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.Output, $"Standard Output:\n{stdOutBuffer}");

            if (stdErrBuffer.Length > 0)
                ConversionLogger.LogIfDebug(conversion, ConversionStatus.ErrorOutput,
                    $"Standard Error:\n{stdErrBuffer}");

            var status = IsExitCodeAcceptable(executablePath, exitCode)
                ? AdRastaStatus.Success
                : AdRastaStatus.UnknownError;

            return new ProcessRunResult
            {
                Conversion = conversion,
                Status = status,
                ExitCode = exitCode,
                StandardOutput = stdOutBuffer.ToString(),
                StandardError = stdErrBuffer.ToString()
            };
        }
        catch (Exception e)
        {
            conversion.ProcessID = 0;
            ConversionLogger.LogIfDebug(conversion, ConversionStatus.Error,
                $"Process failed — {Path.GetFileName(executablePath)} (from {callerName})\nException: {e}");

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

    private static bool IsExitCodeAcceptable(string executablePath, int exitCode)
    {
        var toolName = Path.GetFileName(executablePath).ToLowerInvariant();

        return toolName switch
        {
            "mads.exe" => exitCode is 0,
            "rastaconverter.exe" => exitCode is 0 or 1 or -1,
            "rc2mch.exe" => exitCode == 0,
            _ => exitCode == 0
        };
    }
}