using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;
using Peerly.Core.Identifiers;
using Peerly.Core.Pagination;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchStudentCourses;

public record SearchStudentCoursesQuery : IQuery<SearchStudentCoursesQueryResponse>
{
    public required StudentId StudentId { get; init; }
    public required SearchCoursesQueryFilter Filter { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
}

