using System;
using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Courses;
using Peerly.Core.Persistence.Repositories.Courses.Models;
using Peerly.Core.Tools;

namespace Peerly.Core.Persistence.Repositories.Courses;

internal static class CourseRepositoryMapper
{
    public static IReadOnlyCollection<Course> ToCourses(this IEnumerable<CourseDb> courseDbs)
    {
        return courseDbs.ToArrayBy(
            courseDb => new Course
            {
                Id = new CourseId(courseDb.Id),
                Name = courseDb.Name,
                Description = courseDb.Description,
                Status = Enum.Parse<CourseStatus>(courseDb.Status)
            });
    }
}
