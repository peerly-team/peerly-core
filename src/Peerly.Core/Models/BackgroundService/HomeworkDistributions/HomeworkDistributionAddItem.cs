using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.BackgroundService.HomeworkDistributions;

public sealed record HomeworkDistributionAddItem
{
    public required HomeworkId HomeworkId { get; init; }
    public required DateTimeOffset DistributionTime { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
    public required ProcessStatus ProcessStatus { get; init; }
    public required int FailCount { get; init; }
}
