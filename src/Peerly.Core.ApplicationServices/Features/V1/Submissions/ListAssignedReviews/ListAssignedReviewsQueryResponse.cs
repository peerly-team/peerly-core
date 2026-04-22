using System.Collections.Generic;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;

public sealed record ListAssignedReviewsQueryResponse
{
    public required IReadOnlyCollection<AssignedReview> AssignedReviews { get; init; }
}
