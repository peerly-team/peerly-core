using System;

namespace Peerly.Core.Persistence.Repositories.HomeworkDistributions.Models;

internal sealed record HomeworkDistributionJobItemDb
{
    public required long HomeworkId { get; init; }
    public required DateTimeOffset DistributionTime { get; init; }
}
