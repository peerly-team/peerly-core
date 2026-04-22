using Peerly.Core.Identifiers;
using Peerly.Core.Models.Submissions;
using Peerly.Core.Persistence.Repositories.SubmittedHomeworkMarks.Models;

namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworkMarks;

internal static class SubmittedHomeworkMarkRepositoryMapper
{
    public static SubmittedHomeworkMark ToSubmittedHomeworkMark(this SubmittedHomeworkMarkDb db)
    {
        return new SubmittedHomeworkMark
        {
            SubmittedHomeworkId = new SubmittedHomeworkId(db.SubmittedHomeworkId),
            ReviewersMark = db.ReviewersMark,
            TeacherMark = db.TeacherMark,
            HasDiscrepancy = db.HasDiscrepancy
        };
    }
}
