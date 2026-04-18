using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.ListStudentCourseHomeworks;

internal static class ListStudentCourseHomeworksHandlerMapper
{
    private static readonly HomeworkStatus[] s_visibleStatuses =
    [
        HomeworkStatus.Published,
        HomeworkStatus.Reviewing,
        HomeworkStatus.Confirmation,
        HomeworkStatus.Finished
    ];

    public static GroupFilter ToGroupFilter(this ListStudentCourseHomeworksQuery query)
    {
        return GroupFilter.Empty() with { CourseIds = [query.CourseId] };
    }

    public static GroupStudentFilter ToGroupStudentFilter(this ListStudentCourseHomeworksQuery query, IReadOnlyCollection<GroupId> groupIds)
    {
        return new GroupStudentFilter
        {
            GroupIds = groupIds,
            StudentIds = [query.StudentId]
        };
    }

    public static HomeworkFilter ToGroupHomeworkFilter(this ListStudentCourseHomeworksQuery query, IReadOnlyCollection<GroupId> studentGroupIds)
    {
        return new HomeworkFilter
        {
            CourseIds = [query.CourseId],
            GroupIds = studentGroupIds,
            HomeworkStatuses = s_visibleStatuses
        };
    }

    public static HomeworkFilter ToCourseHomeworkFilter(this ListStudentCourseHomeworksQuery query)
    {
        return HomeworkFilter.Empty() with
        {
            CourseIds = [query.CourseId],
            HomeworkStatuses = s_visibleStatuses
        };
    }
}
