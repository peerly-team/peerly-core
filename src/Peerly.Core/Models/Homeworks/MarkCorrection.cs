using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Homeworks;

public sealed record MarkCorrection
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required int TeacherMark { get; init; }
}
