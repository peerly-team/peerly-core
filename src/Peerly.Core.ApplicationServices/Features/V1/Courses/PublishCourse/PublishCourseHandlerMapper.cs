using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.PublishCourse;

internal static class PublishCourseHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this PublishCourseCommand command)
    {
        return new CourseTeacher
        {
            CourseId = command.CourseId,
            TeacherId = command.TeacherId
        };
    }
}
