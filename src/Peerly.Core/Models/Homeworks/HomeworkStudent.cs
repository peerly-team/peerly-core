using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Homeworks;

public sealed record HomeworkStudent
{
    public required HomeworkId HomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
}
