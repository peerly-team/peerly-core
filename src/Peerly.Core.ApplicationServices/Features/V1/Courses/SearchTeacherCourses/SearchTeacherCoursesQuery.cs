using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Courses.Shared.SearchCourses;
using Peerly.Core.Identifiers;
using Peerly.Core.Pagination;

namespace Peerly.Core.ApplicationServices.Features.V1.Courses.SearchTeacherCourses;

public sealed record SearchTeacherCoursesQuery : IQuery<SearchTeacherCoursesQueryResponse>
{
    public required TeacherId TeacherId { get; init; }
    public required SearchCoursesQueryFilter Filter { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
}
