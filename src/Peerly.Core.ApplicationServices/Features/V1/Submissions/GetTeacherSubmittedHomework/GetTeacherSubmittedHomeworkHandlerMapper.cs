using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;

internal static class GetTeacherSubmittedHomeworkHandlerMapper
{
    public static CourseTeacher ToCourseTeacher(this GetTeacherSubmittedHomeworkQuery query, CourseId courseId)
    {
        return new CourseTeacher
        {
            CourseId = courseId,
            TeacherId = query.TeacherId
        };
    }

    public static TeacherSubmittedReview ToTeacherSubmittedReview(this SubmittedReview review, Student reviewer)
    {
        return new TeacherSubmittedReview
        {
            Review = review,
            Reviewer = reviewer
        };
    }
}
