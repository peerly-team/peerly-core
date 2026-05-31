using System;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateGroupHomework;

internal static class CreateGroupHomeworkHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this CreateGroupHomeworkCommand command, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = command.TeacherId
        };
    }

    public static HomeworkAddItem ToHomeworkAddItem(this CreateGroupHomeworkCommand command, CourseId courseId, DateTimeOffset creationTime)
    {
        return new HomeworkAddItem
        {
            CourseId = courseId,
            GroupId = command.GroupId,
            TeacherId = command.TeacherId,
            Name = command.Name,
            Status = HomeworkStatus.Draft,
            AmountOfReviewers = command.AmountOfReviewers,
            Description = command.Description,
            RubricId = command.RubricId,
            Deadline = command.Deadline,
            ReviewDeadline = command.ReviewDeadline,
            DiscrepancyThreshold = command.DiscrepancyThreshold,
            CreationTime = creationTime
        };
    }
}
