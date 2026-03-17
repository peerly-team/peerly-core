using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedHomeworkFileAddItem
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required FileId FileId { get; init; }
}
