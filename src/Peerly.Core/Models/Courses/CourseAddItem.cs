using System;

namespace Peerly.Core.Models.Courses;

public sealed record CourseAddItem
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required CourseStatus Status { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
