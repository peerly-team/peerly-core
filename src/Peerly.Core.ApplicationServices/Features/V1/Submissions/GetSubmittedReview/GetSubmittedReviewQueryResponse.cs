using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedReview;

public sealed record GetSubmittedReviewQueryResponse
{
    public required SubmittedReview SubmittedReview { get; init; }
}
