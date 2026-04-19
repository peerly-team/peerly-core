using System;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Teachers;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent;

internal static class AddGroupStudentHandlerMapper
{
    public static StudentFilter ToStudentFilter(this AddGroupStudentCommand command)
    {
        return new StudentFilter
        {
            StudentIds = [command.StudentId]
        };
    }

    public static TeacherFilter ToTeacherFilter(this AddGroupStudentCommand command)
    {
        return new TeacherFilter
        {
            TeacherIds = [command.TeacherId]
        };
    }

    public static CourseTeacher ToCourseTeacher(this AddGroupStudentCommand command, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = command.TeacherId
        };
    }

    public static GroupStudentFilter ToGroupStudentFilter(this AddGroupStudentCommand command)
    {
        return new GroupStudentFilter
        {
            GroupIds = [command.GroupId],
            StudentIds = [command.StudentId]
        };
    }

    public static GroupStudentAddItem ToGroupStudentAddItem(this AddGroupStudentCommand command, DateTimeOffset currentTime)
    {
        return new GroupStudentAddItem
        {
            GroupId = command.GroupId,
            StudentId = command.StudentId,
            CreationTime = currentTime
        };
    }
}
