using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedReview;

public sealed record GetSubmittedReviewQuery : IQuery<GetSubmittedReviewQueryResponse>
{
    public required SubmittedReviewId SubmittedReviewId { get; init; }
    public required StudentId StudentId { get; init; }
}
