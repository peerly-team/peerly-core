using Peerly.Core.Identifiers;
using Peerly.Core.Models.Homeworks;
using Peerly.Core.Persistence.Repositories.SubmittedHomeworks.Models;

namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworks;

internal static class SubmittedHomeworkRepositoryMapper
{
    public static SubmittedHomeworkStudent ToSubmittedHomeworkStudent(this SubmittedHomeworkStudentDb submittedHomeworkStudentDb)
    {
        return new SubmittedHomeworkStudent
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(submittedHomeworkStudentDb.Id),
            StudentId = new StudentId(submittedHomeworkStudentDb.StudentId)
        };
    }
}
