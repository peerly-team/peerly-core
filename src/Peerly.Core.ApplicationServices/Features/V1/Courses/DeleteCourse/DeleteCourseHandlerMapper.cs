using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;

internal static class DeleteCourseHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this DeleteCourseCommand command)
    {
        return new CourseTeacher
        {
            CourseId = command.CourseId,
            TeacherId = command.TeacherId
        };
    }
}
