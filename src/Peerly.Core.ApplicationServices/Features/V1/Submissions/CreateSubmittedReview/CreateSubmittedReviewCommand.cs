using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedReview;

public sealed record CreateSubmittedReviewCommand : ICommand<CreateSubmittedReviewCommandResponse>
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
    public required int Mark { get; init; }
    public required string Comment { get; init; }
}
