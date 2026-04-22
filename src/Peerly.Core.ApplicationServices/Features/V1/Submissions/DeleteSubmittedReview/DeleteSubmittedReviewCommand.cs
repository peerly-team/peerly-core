using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedReview;

public sealed record DeleteSubmittedReviewCommand : ICommand<Success>
{
    public required SubmittedReviewId SubmittedReviewId { get; init; }
    public required StudentId StudentId { get; init; }
}
