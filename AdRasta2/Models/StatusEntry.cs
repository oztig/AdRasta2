using System;
using AdRasta2.Enums;

namespace AdRasta2.Models;

public class StatusEntry
{
    public ConversionStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    
    public string TooltipText => $"{Status}: {Timestamp:dd/MM HH:mm}";

    public override string ToString()
    {
        return $"{Status}: {Timestamp:dd/MM HH:mm}";
    }
}
