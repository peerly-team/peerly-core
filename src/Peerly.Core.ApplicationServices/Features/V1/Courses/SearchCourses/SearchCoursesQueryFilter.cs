using System.Collections.Generic;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchCourses;

public sealed record SearchCoursesQueryFilter
{
    public required IReadOnlyCollection<CourseStatus> CourseStatuses { get; init; }
}
