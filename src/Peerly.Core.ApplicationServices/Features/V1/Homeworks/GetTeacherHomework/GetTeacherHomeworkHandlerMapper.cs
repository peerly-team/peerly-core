using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.GetTeacherHomework;

internal static class GetTeacherHomeworkHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this GetTeacherHomeworkQuery query, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = query.TeacherId
        };
    }

    public static TeacherHomeworkInfo ToTeacherHomeworkInfo(this Homework homework)
    {
        return new TeacherHomeworkInfo
        {
            Id = homework.Id,
            Name = homework.Name,
            Status = homework.Status,
            CheckList = homework.CheckList,
            Description = homework.Description,
            Deadline = homework.Deadline,
            ReviewDeadline = homework.ReviewDeadline,
            AmountOfReviewers = homework.AmountOfReviewers,
            DiscrepancyThreshold = homework.DiscrepancyThreshold
        };
    }
}
