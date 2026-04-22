using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.UpdateSubmittedReview;

public sealed record UpdateSubmittedReviewCommand : ICommand<Success>
{
    public required SubmittedReviewId SubmittedReviewId { get; init; }
    public required StudentId StudentId { get; init; }
    public required int Mark { get; init; }
    public required string Comment { get; init; }
}
