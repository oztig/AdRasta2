using System.Collections.Generic;
using AdRasta2.Enums;

namespace AdRasta2.Models;

public static class StatusFilter
{
    private static readonly HashSet<ConversionStatus> ExcludedStatuses = new()
    {
        ConversionStatus.SourceAdded,
        ConversionStatus.MaskAdded,
        ConversionStatus.SourceCleared,
        ConversionStatus.DestinationSet
    };

    public static bool ShouldIncludeOnImageDotLine(ConversionStatus status)
    {
        return !ExcludedStatuses.Contains(status);
    }
}