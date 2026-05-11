using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Teachers;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchStudentCourses;

public sealed record SearchStudentCoursesQueryResponse
{
    public required IReadOnlyCollection<Course> Courses { get; init; }
    public required IReadOnlyDictionary<CourseId, IReadOnlyCollection<Teacher>> TeachersByCourseId { get; init; }
}
