using System.Collections.Generic;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchCourses;

public sealed record SearchCoursesQueryResponse
{
    public required IReadOnlyCollection<SearchCoursesQueryResponseItem> CourseInfos { get; init; }
}
