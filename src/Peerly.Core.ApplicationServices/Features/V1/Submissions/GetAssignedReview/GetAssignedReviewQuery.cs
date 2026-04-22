using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;

public sealed record GetAssignedReviewQuery : IQuery<GetAssignedReviewQueryResponse>
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
}
