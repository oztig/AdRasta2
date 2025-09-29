using System;
using AdRasta2.Enums;
using System.Diagnostics;
using AdRasta2.Models;

namespace AdRasta2.Utils;

public static class ConversionLogger
{
    public static void Log(RastaConversion conversion, ConversionStatus status, string message, Exception ex = null)
    {
        if (conversion == null)
            return;

        var fullMessage = ex == null ? message : $"{message} — {ex.Message}";
        conversion.Statuses.AddEntry(DateTime.Now, status, fullMessage);

        // var prefix = status switch
        // {
        //     ConversionStatus.Failed => "[ERROR]",
        //     ConversionStatus.Warning => "[WARNING]",
        //     ConversionStatus.Info => "[INFO]",
        //     ConversionStatus.Running => "[RUNNING]",
        //     ConversionStatus.Completed => "[DONE]",
        //     _ => "[LOG]"
        // };
        //
        // Debug.WriteLine($"{prefix} {conversion.Name}: {fullMessage}");
    }
    
    public static void LogIfDebug(RastaConversion conversion, ConversionStatus status, string message, Exception ex = null)
    {
        if (!Settings.DebugMode)
            return;

        Log(conversion, status, message,ex);
    }

}