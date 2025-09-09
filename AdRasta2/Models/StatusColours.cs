using System.Collections.Generic;
using AdRasta2.Enums;
using Avalonia.Media;

namespace AdRasta2.Models;

public static class StatusColours
{
    public static readonly Dictionary<ConversionStatus, Color> Map = new()
    {
        { ConversionStatus.PreviewGenerated, Colors.SkyBlue },
        { ConversionStatus.ConversionActive, Colors.Orange },
        { ConversionStatus.ConversionComplete ,Colors.LimeGreen},
        { ConversionStatus.XexGenerated, Colors.Blue },
        { ConversionStatus.MCHGenerated, Colors.MediumPurple }
    };
}
