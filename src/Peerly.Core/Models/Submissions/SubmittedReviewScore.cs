using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedReviewScore
{
    public required SubmittedReviewScoreId Id { get; init; }
    public required SubmittedReviewId SubmittedReviewId { get; init; }
    public required RubricCriterionId RubricCriterionId { get; init; }
    public required int Score { get; init; }
    public string? Comment { get; init; }
}
