using System.Collections.Generic;
using AdRasta2.Enums;

namespace AdRasta2.Models;

public static class StatusFilter
{
    private static readonly HashSet<ConversionStatus> ExcludedStatuses = new()
    {
        ConversionStatus.SourceAdded,
        ConversionStatus.SourceCleared,
        ConversionStatus.MaskAdded,
        ConversionStatus.MaskCleared,
        ConversionStatus.DestinationSet,
        ConversionStatus.PreviewStarted,
        ConversionStatus.ConversionStarted,
        ConversionStatus.Error,
        ConversionStatus.Warning,
        ConversionStatus.Info,
        ConversionStatus.Running,
        ConversionStatus.Completed
    };

    public static bool ShouldIncludeOnImageDotLine(ConversionStatus status)
    {
        return !ExcludedStatuses.Contains(status);
    }
}