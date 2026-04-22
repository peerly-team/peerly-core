using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;

public sealed record CreateSubmittedReviewCommandResponse
{
    public required SubmittedReviewId SubmittedReviewId { get; init; }
}
