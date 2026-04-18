using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.BackgroundService.ReviewCompletions;

public sealed record ReviewCompletionJobItem
{
    public required HomeworkId HomeworkId { get; init; }
    public required DateTimeOffset CompletionTime { get; init; }
}
