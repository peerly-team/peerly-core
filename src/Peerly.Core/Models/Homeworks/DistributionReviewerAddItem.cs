using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Homeworks;

public sealed record DistributionReviewerAddItem
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
}
