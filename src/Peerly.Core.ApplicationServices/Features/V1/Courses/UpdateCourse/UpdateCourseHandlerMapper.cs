using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.UpdateCourse;

internal static class UpdateCourseHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this UpdateCourseCommand command)
    {
        return new CourseTeacher
        {
            CourseId = command.CourseId,
            TeacherId = command.TeacherId
        };
    }
}
