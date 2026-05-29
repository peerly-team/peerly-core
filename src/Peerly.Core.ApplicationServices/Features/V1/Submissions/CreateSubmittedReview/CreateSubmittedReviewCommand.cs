using System.Collections.Generic;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Submissions.Shared.Models;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;

public sealed record CreateSubmittedReviewCommand : ICommand<CreateSubmittedReviewCommandResponse>
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
    public required IReadOnlyCollection<SubmittedReviewScoreItem> Scores { get; init; }
    public required string Comment { get; init; }
}
