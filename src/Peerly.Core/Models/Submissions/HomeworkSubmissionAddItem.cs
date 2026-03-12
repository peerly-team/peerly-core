using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record HomeworkSubmissionAddItem
{
    public required HomeworkId HomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
    public required string Comment { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
