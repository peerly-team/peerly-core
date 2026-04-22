using System;
using System.Collections.Generic;

namespace Peerly.Core.Models.BackgroundService.HomeworkDistributions;

public sealed record HomeworkDistributionFilter
{
    public required IReadOnlyCollection<ProcessStatus> ProcessStatuses { get; init; }
    public required int? MaxFailCount { get; init; }
    public required TimeSpan? ProcessTimeoutSeconds { get; init; }
    public required int? Limit { get; init; }

    public static HomeworkDistributionFilter Empty()
    {
        return new HomeworkDistributionFilter
        {
            ProcessStatuses = [],
            MaxFailCount = null,
            ProcessTimeoutSeconds = null,
            Limit = null
        };
    }
}
