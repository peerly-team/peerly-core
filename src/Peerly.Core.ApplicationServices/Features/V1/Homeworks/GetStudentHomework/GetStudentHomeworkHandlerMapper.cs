using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;

internal static class GetStudentHomeworkHandlerMapper
{
    public static GroupFilter ToCourseGroupFilter(this GetStudentHomeworkQuery query, CourseId courseId)
    {
        return GroupFilter.Empty() with
        {
            CourseIds = [courseId]
        };
    }

    public static GroupStudentFilter ToGroupStudentFilter(
        this GetStudentHomeworkQuery query,
        IReadOnlyCollection<GroupId> courseGroupIds)
    {
        return new GroupStudentFilter
        {
            GroupIds = courseGroupIds,
            StudentIds = [query.StudentId]
        };
    }

    public static HomeworkStudent ToHomeworkStudent(this GetStudentHomeworkQuery query, HomeworkId homeworkId)
    {
        return new HomeworkStudent
        {
            HomeworkId = homeworkId,
            StudentId = query.StudentId
        };
    }
}
