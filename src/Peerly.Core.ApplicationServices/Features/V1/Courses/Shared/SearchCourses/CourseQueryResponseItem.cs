namespace Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;

public sealed record CourseQueryResponseItem
{
    public required Core.Models.Courses.Course Course { get; init; }
    public required long StudentCount { get; init; }
    public required long HomeworkCount { get; init; }
}
