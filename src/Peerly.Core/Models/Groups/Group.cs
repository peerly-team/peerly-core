using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Groups;

public sealed record Group
{
    public required GroupId Id { get; init; }
    public required CourseId CourseId { get; init; }
    public required string Name { get; init; }
    public required int StudentCount { get; init; }
}
