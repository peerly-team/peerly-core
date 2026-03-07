using System.Collections.Generic;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchStudentCourses;

public sealed record SearchStudentCoursesQueryResponse
{
    public required IReadOnlyCollection<SearchCoursesQueryResponseItem> CourseInfos { get; init; }
}
