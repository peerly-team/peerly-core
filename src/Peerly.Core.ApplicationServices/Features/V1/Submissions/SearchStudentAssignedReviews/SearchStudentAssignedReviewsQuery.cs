using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.SearchStudentAssignedReviews;

public sealed record SearchStudentAssignedReviewsQuery : IQuery<SearchStudentAssignedReviewsQueryResponse>
{
    public required StudentId StudentId { get; init; }
    public required HomeworkId HomeworkId { get; init; }
}
