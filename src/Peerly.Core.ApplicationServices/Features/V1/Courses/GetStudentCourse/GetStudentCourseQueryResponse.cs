using System.Collections.Generic;
using Peerly.Core.Models.Courses;
using Peerly.Core.Models.Files;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.GetStudentCourse;

public sealed record GetStudentCourseQueryResponse
{
    public required Course Course { get; init; }
    public required long StudentCount { get; init; }
    public required long HomeworkCount { get; init; }
    public required IReadOnlyCollection<File> Files { get; init; }
}
