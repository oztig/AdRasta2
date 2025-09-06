using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AdRasta2.Utils;

public static class SafeCommand
{
    public static string QuoteIfNeeded(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "\"\"";

        bool needsQuotes = value.Any(c => char.IsWhiteSpace(c) || "\"&()".Contains(c));

        if (!needsQuotes)
            return value;

        if (OperatingSystem.IsWindows())
        {
            // Escape double quotes for CMD/PowerShell
            value = value.Replace("\"", "\\\"");
            return $"\"{value}\"";
        }
        else
        {
            // Escape double quotes for bash-compatible shells
            value = value.Replace("\"", "\\\"");
            return $"\"{value}\"";
        }
    }
}