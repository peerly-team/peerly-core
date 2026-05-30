using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Participants;

public sealed record SkippedStudentInfo
{
    public required StudentId Id { get; init; }
    public required SkippedStudentReason Reason { get; init; }
}
