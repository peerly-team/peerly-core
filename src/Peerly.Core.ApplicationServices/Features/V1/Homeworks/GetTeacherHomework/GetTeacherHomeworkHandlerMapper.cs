using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;

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

    public static SubmittedHomeworkFilter ToSubmittedHomeworkFilter(this GetTeacherHomeworkQuery query, HomeworkId homeworkId)
    {
        return new SubmittedHomeworkFilter
        {
            HomeworkIds = [homeworkId]
        };
    }
}
