using System;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateCourseHomework;

internal static class CreateCourseHomeworkHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this CreateCourseHomeworkCommand command)
    {
        return new CourseTeacher
        {
            CourseId = command.CourseId,
            TeacherId = command.TeacherId
        };
    }

    public static HomeworkAddItem ToHomeworkAddItem(this CreateCourseHomeworkCommand command, DateTimeOffset creationTime)
    {
        return new HomeworkAddItem
        {
            CourseId = command.CourseId,
            TeacherId = command.TeacherId,
            Name = command.Name,
            Status = HomeworkStatus.Draft,
            AmountOfReviewers = command.AmountOfReviewers,
            Description = command.Description,
            Checklist = command.Checklist,
            Deadline = command.Deadline,
            ReviewDeadline = command.ReviewDeadline,
            DiscrepancyThreshold = command.DiscrepancyThreshold,
            CreationTime = creationTime
        };
    }
}
