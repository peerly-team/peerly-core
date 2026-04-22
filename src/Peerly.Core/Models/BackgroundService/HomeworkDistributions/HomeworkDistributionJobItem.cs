using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.BackgroundService.HomeworkDistributions;

public sealed record HomeworkDistributionJobItem
{
    public required HomeworkId HomeworkId { get; init; }
    public required DateTimeOffset DistributionTime { get; init; }
}
