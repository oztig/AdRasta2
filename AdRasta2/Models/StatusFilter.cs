using System.Collections.Generic;
using AdRasta2.Enums;

namespace AdRasta2.Models;

public static class StatusFilter
{
    private static readonly HashSet<ConversionStatus> ExcludedStatuses = new()
    {
        ConversionStatus.MCHGenerated
        // Add others as needed
    };

    public static bool ShouldIncludeOnImageDotLine(ConversionStatus status)
    {
        return !ExcludedStatuses.Contains(status);
    }
}
