using System.Collections.Generic;
using AdRasta2.Enums;
using Avalonia.Media;

namespace AdRasta2.Models;

public static class StatusColours
{
    public static readonly Dictionary<ConversionStatus, Color> Map = new()
    {
        { ConversionStatus.SourceAdded, Colors.LightCyan },
        { ConversionStatus.MaskAdded, Colors.Cyan },
        { ConversionStatus.PreviewGenerated, Colors.Orange },
        { ConversionStatus.ConversionContinued, Colors.Coral },
        { ConversionStatus.ConversionStarted, Colors.Green },
        { ConversionStatus.ConversionComplete ,Colors.LimeGreen },
        { ConversionStatus.ExecutableGenerated, Colors.Blue },
        { ConversionStatus.MCHGenerated, Colors.MediumPurple }
    };
}
