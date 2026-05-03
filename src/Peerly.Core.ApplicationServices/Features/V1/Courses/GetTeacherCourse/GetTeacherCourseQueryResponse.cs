using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetTeacherCourse;

public sealed record GetTeacherCourseQueryResponse
{
    public required Course Course { get; init; }
    public required long StudentCount { get; init; }
    public required long HomeworkCount { get; init; }
}
