using System.Collections.Generic;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchTeacherCourses;

public sealed record SearchTeacherCoursesQueryResponse
{
    public required IReadOnlyCollection<Course> Courses { get; init; }
}
