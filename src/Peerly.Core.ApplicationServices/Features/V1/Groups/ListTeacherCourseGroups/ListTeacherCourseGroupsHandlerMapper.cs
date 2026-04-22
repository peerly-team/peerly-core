using System.Collections.Generic;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Tools;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.ListTeacherCourseGroups;

internal static class ListTeacherCourseGroupsHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this ListTeacherCourseGroupsQuery query)
    {
        return new CourseTeacher
        {
            CourseId = query.CourseId,
            TeacherId = query.TeacherId
        };
    }

    public static GroupFilter ToGroupFilter(this ListTeacherCourseGroupsQuery query)
    {
        return GroupFilter.Empty() with
        {
            CourseIds = [query.CourseId]
        };
    }

    public static GroupTeacherFilter ToGroupTeacherFilter(this ListTeacherCourseGroupsQuery query, IReadOnlyCollection<Group> groupsOfCourse)
    {
        return new GroupTeacherFilter
        {
            TeacherIds = [query.TeacherId],
            GroupIds = groupsOfCourse.ToArrayBy(group => group.Id)
        };
    }
}
