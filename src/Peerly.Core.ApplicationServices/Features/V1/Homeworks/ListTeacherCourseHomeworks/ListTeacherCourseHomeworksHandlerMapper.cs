using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListTeacherCourseHomeworks;

internal static class ListTeacherCourseHomeworksHandlerMapper
{
    public static CourseTeacherExistsItem ToCourseTeacherExistsItem(this ListTeacherCourseHomeworksQuery query)
    {
        return new CourseTeacherExistsItem
        {
            CourseId = query.CourseId,
            TeacherId = query.TeacherId
        };
    }

    public static HomeworkFilter ToHomeworkFilter(this ListTeacherCourseHomeworksQuery query)
    {
        return HomeworkFilter.Empty() with { CourseIds = [query.CourseId] };
    }
}
