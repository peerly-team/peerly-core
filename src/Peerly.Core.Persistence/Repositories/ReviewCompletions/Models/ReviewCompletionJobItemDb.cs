using System;

namespace Peerly.Core.Persistence.Repositories.ReviewCompletions.Models;

internal sealed record ReviewCompletionJobItemDb
{
    public required long HomeworkId { get; init; }
    public required DateTimeOffset CompletionTime { get; init; }
}
