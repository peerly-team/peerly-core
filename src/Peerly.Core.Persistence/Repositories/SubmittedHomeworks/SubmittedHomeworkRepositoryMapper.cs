using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.Repositories.SubmittedHomeworks.Models;
using SubmittedHomeworkStudent = Peerly.Core.Models.Homeworks.SubmittedHomeworkStudent;

namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworks;

internal static class SubmittedHomeworkRepositoryMapper
{
    public static SubmittedHomework ToSubmittedHomework(this SubmittedHomeworkDb db)
    {
        return new SubmittedHomework
        {
            Id = new SubmittedHomeworkId(db.Id),
            HomeworkId = new HomeworkId(db.HomeworkId),
            StudentId = new StudentId(db.StudentId),
            Comment = db.Comment
        };
    }

    public static SubmittedHomeworkStudent ToSubmittedHomeworkStudent(this SubmittedHomeworkStudentDb submittedHomeworkStudentDb)
    {
        return new SubmittedHomeworkStudent
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(submittedHomeworkStudentDb.Id),
            StudentId = new StudentId(submittedHomeworkStudentDb.StudentId)
        };
    }
}
