using System.Collections.Generic;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchCourses;

public sealed record SearchCoursesQueryResponse
{
    public required IReadOnlyCollection<SearchCoursesQueryResponseItem> CourseInfos { get; init; }
}

public sealed record SearchCoursesQueryResponseItem
{
    public required Course Course { get; init; }
    public required long StudentCount { get; init; }
    public required long HomeworkCount { get; init; }
}
