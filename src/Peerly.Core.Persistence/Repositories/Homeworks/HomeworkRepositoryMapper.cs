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
            HomeworkId = new HomeworkId(homeworkDb.HomeworkId),
            CourseId = new CourseId(homeworkDb.CourseId),
            TeacherId = new TeacherId(homeworkDb.TeacherId),
            Name = homeworkDb.Name,
            Description = homeworkDb.Description,
            CheckList = homeworkDb.CheckList,
            Deadline = homeworkDb.Deadline,
            ReviewDeadline = homeworkDb.ReviewDeadline
        };
    }
}
