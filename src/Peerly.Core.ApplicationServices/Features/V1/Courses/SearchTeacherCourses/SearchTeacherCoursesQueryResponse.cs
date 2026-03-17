using System.Collections.Generic;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchTeacherCourses;

public sealed record SearchTeacherCoursesQueryResponse
{
    public required IReadOnlyCollection<CourseQueryResponseItem> CourseInfos { get; init; }
}
