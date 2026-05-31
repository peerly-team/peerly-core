using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.Shared.Models;

public sealed record SubmittedReviewScoreItem
{
    public required RubricCriterionId RubricCriterionId { get; init; }
    public required int Score { get; init; }
    public string? Comment { get; init; }
}
