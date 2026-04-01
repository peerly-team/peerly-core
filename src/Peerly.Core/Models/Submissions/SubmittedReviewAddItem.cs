using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedReviewAddItem
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
    public required int Mark { get; init; }
    public required string Comment { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
