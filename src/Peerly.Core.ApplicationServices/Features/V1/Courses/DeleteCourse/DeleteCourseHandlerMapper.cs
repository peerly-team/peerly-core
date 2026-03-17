using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.DeleteCourse;

internal static class DeleteCourseHandlerMapper
{
    public static CourseTeacherExistsItem ToCourseTeacherExistsItem(this DeleteCourseCommand command)
    {
        return new CourseTeacherExistsItem
        {
            CourseId = command.CourseId,
            TeacherId = command.TeacherId
        };
    }
}
