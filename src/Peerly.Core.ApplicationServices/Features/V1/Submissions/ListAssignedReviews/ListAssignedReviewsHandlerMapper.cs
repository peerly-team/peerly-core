using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Groups;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.ListAssignedReviews;

internal static class ListAssignedReviewsHandlerMapper
{
    public static GroupStudent ToGroupStudent(this ListAssignedReviewsQuery query, GroupId groupId)
    {
        return new GroupStudent
        {
            GroupId = groupId,
            StudentId = query.StudentId
        };
    }

    public static CourseStudent ToCourseStudent(this ListAssignedReviewsQuery query, CourseId courseId)
    {
        return new CourseStudent
        {
            CourseId = courseId,
            StudentId = query.StudentId
        };
    }

    public static HomeworkStudent ToHomeworkStudent(this ListAssignedReviewsQuery query)
    {
        return new HomeworkStudent
        {
            HomeworkId = query.HomeworkId,
            StudentId = query.StudentId
        };
    }
}
