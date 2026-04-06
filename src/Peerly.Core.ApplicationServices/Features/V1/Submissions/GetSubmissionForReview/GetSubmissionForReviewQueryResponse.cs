using System.Collections.Generic;
using Peerly.Core.Models.Files;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmissionForReview;

public sealed record GetSubmissionForReviewQueryResponse
{
    public required long SubmittedHomeworkId { get; init; }
    public required string Comment { get; init; }
    public required IReadOnlyCollection<File> AnonymizedFiles { get; init; }
    public required string Checklist { get; init; }
}
