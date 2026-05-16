using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListStudentCourseHomeworks;

internal static class ListStudentCourseHomeworksHandlerMapper
{
    public static CourseStudent ToCourseStudent(this ListStudentCourseHomeworksQuery query)
    {
        return new CourseStudent
        {
            CourseId = query.CourseId,
            StudentId = query.StudentId
        };
    }
}
