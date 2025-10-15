using System;
using AdRasta2.Enums;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using AdRasta2.Models;

namespace AdRasta2.Utils;

public static class ConversionLogger
{
    public static void Log(RastaConversion conversion, ConversionStatus status, string message, Exception ex = null)
    {
        var fullMessage = ex == null ? message : $"{message} — {ex.Message}";
        conversion.Statuses.AddEntry(DateTime.Now, status, fullMessage);

    }
    
    public static void LogIfDebug(
        RastaConversion conversion,
        ConversionStatus status,
        string message,
        Exception? ex = null,
        [CallerMemberName] string caller = "",
        bool forceDebug=false)
    {
        if (!Settings.DebugMode && !forceDebug )
            return;
        
        Log(conversion, status, $"{message} (from {caller})", ex);
    }

}