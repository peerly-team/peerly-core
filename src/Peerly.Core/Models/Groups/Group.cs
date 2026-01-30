namespace Peerly.Core.Models.Groups;

public sealed record Group
{
    public required long Id { get; init; }
    public required long CourseId { get; init; }
    public required string Name { get; init; }
}
