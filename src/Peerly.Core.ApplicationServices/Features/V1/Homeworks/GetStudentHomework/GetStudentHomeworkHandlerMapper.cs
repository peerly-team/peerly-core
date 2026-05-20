using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetStudentHomework;

internal static class GetStudentHomeworkHandlerMapper
{
    public static GroupStudent ToGroupStudent(this GetStudentHomeworkQuery query, GroupId groupId)
    {
        return new GroupStudent
        {
            GroupId = groupId,
            StudentId = query.StudentId
        };
    }

    public static CourseStudent ToCourseStudent(this GetStudentHomeworkQuery query, CourseId courseId)
    {
        return new CourseStudent
        {
            CourseId = courseId,
            StudentId = query.StudentId
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
