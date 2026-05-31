using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedReviewScoreAddItem
{
    public required SubmittedReviewId SubmittedReviewId { get; init; }
    public required RubricCriterionId RubricCriterionId { get; init; }
    public required int Score { get; init; }
    public string? Comment { get; init; }
}
