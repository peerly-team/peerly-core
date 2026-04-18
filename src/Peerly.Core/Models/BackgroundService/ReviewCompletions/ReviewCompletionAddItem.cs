using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.BackgroundService.ReviewCompletions;

public sealed record ReviewCompletionAddItem
{
    public required HomeworkId HomeworkId { get; init; }
    public required DateTimeOffset CompletionTime { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
    public required ProcessStatus ProcessStatus { get; init; }
    public required int FailCount { get; init; }
}
