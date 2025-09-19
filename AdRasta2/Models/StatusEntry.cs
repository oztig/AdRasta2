using System;
using AdRasta2.Converters;
using AdRasta2.Enums;
using Avalonia.Media;

namespace AdRasta2.Models;

public class StatusEntry
{
    public ConversionStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
    
    public bool ShowOnImageStatusLine { get; set; }
    
    public string TooltipText => $"{Status}: {Timestamp:dd/MM HH:mm:ss}";

    public string Message => ToString();
    
    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Details)
            ? TooltipText
            : $"{Status}: {Timestamp:dd/MM HH:mm:ss} — {Details}";
    }
}
