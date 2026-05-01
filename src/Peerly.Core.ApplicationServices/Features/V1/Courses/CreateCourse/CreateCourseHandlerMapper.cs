using System;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Teachers;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.CreateCourse;

internal static class CreateCourseHandlerMapper
{
    public static CourseTeacherAddItem ToCourseTeacherAddItem(this CreateCourseCommand command, CourseId courseId, DateTimeOffset creationTime)
    {
        return new CourseTeacherAddItem
        {
            CourseId = courseId,
            TeacherId = command.TeacherId,
            CreationTime = creationTime
        };
    }

    public static CourseAddItem ToCourseAddItem(this CreateCourseCommand command, DateTimeOffset creationTime)
    {
        return new CourseAddItem
        {
            Name = command.Name,
            Description = command.Description,
            Status = CourseStatus.Draft,
            CreationTime = creationTime
        };
    }

    public static TeacherFilter ToTeacherFilter(this CreateCourseCommand command)
    {
        return new TeacherFilter
        {
            TeacherIds = [command.TeacherId]
        };
    }
}
