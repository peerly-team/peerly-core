using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.PublishCourse;

public sealed record PublishCourseCommand : ICommand<Success>
{
    public required CourseId CourseId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
