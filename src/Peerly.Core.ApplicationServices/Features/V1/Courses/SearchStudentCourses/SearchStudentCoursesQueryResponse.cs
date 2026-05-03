using System.Collections.Generic;
using Peerly.Core.Models.Courses;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchStudentCourses;

public sealed record SearchStudentCoursesQueryResponse
{
    public required IReadOnlyCollection<Course> Courses { get; init; }
}
