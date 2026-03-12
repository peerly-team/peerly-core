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
            ReviewDeadline = homeworkDb.ReviewDeadline
        };
    }
}
