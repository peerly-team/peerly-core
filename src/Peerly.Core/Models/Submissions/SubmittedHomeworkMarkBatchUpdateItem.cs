using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedHomeworkMarkBatchUpdateItem
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required int TeacherMark { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required DateTimeOffset UpdateTime { get; init; }
}
