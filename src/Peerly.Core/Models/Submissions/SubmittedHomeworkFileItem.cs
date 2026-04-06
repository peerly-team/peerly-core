using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedHomeworkFileItem
{
    public required FileId FileId { get; init; }
    public required FileId? AnonymizedFileId { get; init; }
}
