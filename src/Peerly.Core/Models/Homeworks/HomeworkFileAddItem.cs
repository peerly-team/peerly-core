using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Homeworks;

public sealed record HomeworkFileAddItem
{
    public required HomeworkId HomeworkId { get; init; }
    public required FileId FileId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
