using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListSubmittedHomeworkOverview;

internal static class ListSubmittedHomeworkOverviewHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this ListSubmittedHomeworkOverviewQuery query, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = query.TeacherId
        };
    }

    public static SubmittedHomeworkOverview ToSubmittedHomeworkOverview(
        this SubmittedHomeworkStudent submittedHomeworkStudent,
        Student student,
        SubmittedHomeworkMark? mark,
        int reviewCount)
    {
        return new SubmittedHomeworkOverview
        {
            SubmittedHomeworkId = submittedHomeworkStudent.SubmittedHomeworkId,
            Student = student,
            ReviewCount = reviewCount,
            ReviewersMark = mark?.ReviewersMark,
            HasDiscrepancy = mark?.HasDiscrepancy,
            TeacherMark = mark?.TeacherMark
        };
    }
}
