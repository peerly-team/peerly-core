using System.Collections.Generic;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.SearchStudentAssignedReviews;

public sealed record SearchStudentAssignedReviewsQueryResponse
{
    public required IReadOnlyCollection<AssignedReview> AssignedReviews { get; init; }
}
