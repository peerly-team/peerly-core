namespace Peerly.Core.Models.Courses;

public sealed record CourseUpdateItem
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required CourseStatus Status { get; init; }
}
