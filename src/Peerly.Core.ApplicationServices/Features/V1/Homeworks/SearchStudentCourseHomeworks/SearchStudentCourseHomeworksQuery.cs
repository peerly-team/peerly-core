using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.SearchCourseHomeworks;
using Peerly.Core.Identifiers;
using Peerly.Core.Pagination;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentCourseHomeworks;

public sealed record SearchStudentCourseHomeworksQuery : IQuery<SearchStudentCourseHomeworksQueryResponse>
{
    public required StudentId StudentId { get; init; }
    public required CourseId CourseId { get; init; }
    public required SearchCourseHomeworksQueryFilter Filter { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
}
