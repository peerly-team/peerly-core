using System;

namespace Peerly.Core.Models.BackgroundService.ReviewCompletions;

public sealed record ReviewCompletionUpdateItem
{
    public required DateTimeOffset ProcessTime { get; init; }
    public required ProcessStatus? ProcessStatus { get; init; }
    public required string? Error { get; init; }
    public required bool IncrementFailCount { get; init; }
    public required DateTimeOffset CompletionTime { get; init; }
    public required DateTimeOffset? TakenTime { get; init; }
}
