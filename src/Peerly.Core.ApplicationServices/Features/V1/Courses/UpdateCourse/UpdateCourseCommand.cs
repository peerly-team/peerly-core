using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;

public sealed record UpdateCourseCommand : ICommand<Success>
{
    public required CourseId CourseId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
}
