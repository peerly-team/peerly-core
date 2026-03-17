using Peerly.Core.Abstractions.ApplicationServices;
using Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;

internal sealed class CreateCourseHandlerMapper : ICreateCourseHandlerMapper
{
    private readonly IClock _clock;

    public CreateCourseHandlerMapper(IClock clock)
    {
        _clock = clock;
    }

    public CourseTeacherAddItem ToCourseTeacherAddItem(CreateCourseCommand command, CourseId courseId)
    {
        return new CourseTeacherAddItem
        {
            CourseId = courseId,
            TeacherId = command.TeacherId,
            CreationTime = _clock.GetCurrentTime()
        };
    }

    public CourseAddItem ToCourseAddItem(CreateCourseCommand command)
    {
        return new CourseAddItem
        {
            Name = command.Name,
            Description = command.Description,
            Status = CourseStatus.Draft,
            CreationTime = _clock.GetCurrentTime()
        };
    }
}
