using System;
using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Students;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.BulkAddGroupStudents;

internal static class BulkAddGroupStudentsHandlerMapper
{
    public static StudentFilter ToStudentFilter(this BulkAddGroupStudentsCommand command)
    {
        return new StudentFilter
        {
            StudentIds = command.StudentIds
        };
    }

    public static CourseTeacher ToCourseTeacher(this BulkAddGroupStudentsCommand command, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = command.TeacherId
        };
    }

    public static GroupStudentFilter ToGroupStudentFilter(this BulkAddGroupStudentsCommand command)
    {
        return new GroupStudentFilter
        {
            GroupIds = [command.GroupId],
            StudentIds = command.StudentIds
        };
    }

    public static GroupStudentBulkAddItem ToGroupStudentBulkAddItem(
        this BulkAddGroupStudentsCommand command,
        IReadOnlyCollection<StudentId> studentIds,
        DateTimeOffset currentTime)
    {
        return new GroupStudentBulkAddItem
        {
            GroupId = command.GroupId,
            StudentIds = studentIds,
            CreationTime = currentTime
        };
    }
}
