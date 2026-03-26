using System;

namespace Peerly.Core.Models.BackgroundService.HomeworkDistributions;

public sealed record HomeworkDistributionUpdateItem
{
    public required DateTimeOffset ProcessTime { get; init; }
    public required ProcessStatus? ProcessStatus { get; init; }
    public required string? Error { get; init; }
    public required bool IncrementFailCount { get; init; }
}
