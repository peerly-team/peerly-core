using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedHomework
{
    public required SubmittedHomeworkId Id { get; init; }
    public required HomeworkId HomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
    public required string Comment { get; init; }
}
