using System;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Persistence.Repositories.Homeworks.Models;

namespace Peerly.Core.Persistence.Repositories.Homeworks;

internal static class HomeworkRepositoryMapper
{
    public static Homework ToHomework(this HomeworkDb homeworkDb)
    {
        return new Homework
        {
            Id = new HomeworkId(homeworkDb.Id),
            CourseId = new CourseId(homeworkDb.CourseId),
            GroupId = homeworkDb.GroupId is not null ? new GroupId(homeworkDb.GroupId.Value) : null,
            TeacherId = new TeacherId(homeworkDb.TeacherId),
            Name = homeworkDb.Name,
            Status = Enum.Parse<HomeworkStatus>(homeworkDb.Status),
            AmountOfReviewers = homeworkDb.AmountOfReviewers,
            Description = homeworkDb.Description,
            CheckList = homeworkDb.CheckList,
            Deadline = homeworkDb.Deadline,
            ReviewDeadline = homeworkDb.ReviewDeadline,
            DiscrepancyThreshold = homeworkDb.DiscrepancyThreshold
        };
    }

    public static TeacherHomeworkInfo ToTeacherHomeworkInfo(this TeacherHomeworkInfoDb homeworkDb)
    {
        return new TeacherHomeworkInfo
        {
            Id = new HomeworkId(homeworkDb.Id),
            Name = homeworkDb.Name,
            Status = Enum.Parse<HomeworkStatus>(homeworkDb.Status),
            AmountOfReviewers = homeworkDb.AmountOfReviewers,
            Description = homeworkDb.Description,
            CheckList = homeworkDb.CheckList,
            Deadline = homeworkDb.Deadline,
            ReviewDeadline = homeworkDb.ReviewDeadline,
            DiscrepancyThreshold = homeworkDb.DiscrepancyThreshold
        };
    }

    public static StudentHomeworkInfo ToStudentHomeworkInfo(this StudentHomeworkInfoDb homeworkInfoDb)
    {
        return new StudentHomeworkInfo
        {
            Id = new HomeworkId(homeworkInfoDb.Id),
            Name = homeworkInfoDb.Name,
            Status = Enum.Parse<HomeworkStatus>(homeworkInfoDb.Status),
            AmountOfReviewers = homeworkInfoDb.AmountOfReviewers,
            Description = homeworkInfoDb.Description,
            CheckList = homeworkInfoDb.CheckList,
            Deadline = homeworkInfoDb.Deadline,
            ReviewDeadline = homeworkInfoDb.ReviewDeadline,
            IsHomeworkSubmitted = homeworkInfoDb.IsHomeworkSubmitted
        };
    }
}
