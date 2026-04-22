using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Files;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetAssignedReview;

public sealed record GetAssignedReviewQueryResponse
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required string Comment { get; init; }
    public required string Checklist { get; init; }
    public required IReadOnlyCollection<File> Files { get; init; }
    public required SubmittedReviewId? SubmittedReviewId { get; init; }
}
