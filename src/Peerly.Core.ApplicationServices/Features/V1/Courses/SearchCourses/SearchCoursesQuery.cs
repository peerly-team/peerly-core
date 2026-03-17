using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;
using Peerly.Core.Pagination;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchCourses;

public sealed record SearchCoursesQuery : IQuery<SearchCoursesQueryResponse>
{
    public required SearchCoursesQueryFilter Filter { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
}
