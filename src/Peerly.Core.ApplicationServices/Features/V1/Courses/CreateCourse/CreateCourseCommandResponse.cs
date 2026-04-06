using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;

public sealed record CreateCourseCommandResponse
{
    public required CourseId CourseId { get; init; }
}
