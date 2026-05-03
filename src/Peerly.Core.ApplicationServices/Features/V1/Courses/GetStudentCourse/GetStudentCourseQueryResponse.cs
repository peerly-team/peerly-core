using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;

public sealed record GetStudentCourseQueryResponse
{
    public required Course Course { get; init; }
    public required long StudentCount { get; init; }
    public required long HomeworkCount { get; init; }
}
