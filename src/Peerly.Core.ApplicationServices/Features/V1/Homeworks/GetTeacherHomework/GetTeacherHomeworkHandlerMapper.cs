using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

internal static class GetTeacherHomeworkHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this GetTeacherHomeworkQuery query, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = query.TeacherId
        };
    }
}
