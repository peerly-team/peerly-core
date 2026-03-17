using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Courses;

public sealed record Course
{
    public required CourseId Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required CourseStatus Status { get; init; }
}
