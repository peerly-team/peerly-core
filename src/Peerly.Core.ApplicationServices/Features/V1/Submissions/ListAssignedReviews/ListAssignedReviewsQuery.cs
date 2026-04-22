using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;

public sealed record ListAssignedReviewsQuery : IQuery<ListAssignedReviewsQueryResponse>
{
    public required HomeworkId HomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
}
