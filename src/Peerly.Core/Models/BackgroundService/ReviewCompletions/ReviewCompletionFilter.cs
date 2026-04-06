using System;
using System.Collections.Generic;

namespace Peerly.Core.Models.BackgroundService.ReviewCompletions;

public sealed record ReviewCompletionFilter
{
    public required IReadOnlyCollection<ProcessStatus> ProcessStatuses { get; init; }
    public required int? MaxFailCount { get; init; }
    public required TimeSpan? ProcessTimeoutSeconds { get; init; }
    public required int? Limit { get; init; }

    public static ReviewCompletionFilter Empty()
    {
        return new ReviewCompletionFilter
        {
            ProcessStatuses = [],
            MaxFailCount = null,
            ProcessTimeoutSeconds = null,
            Limit = null
        };
    }
}
