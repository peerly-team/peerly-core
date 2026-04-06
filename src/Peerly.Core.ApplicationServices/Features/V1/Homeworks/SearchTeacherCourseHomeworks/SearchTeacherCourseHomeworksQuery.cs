using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.ApplicationServices.Features.V1.Homeworks.Shared.SearchCourseHomeworks;
using Peerly.Core.Identifiers;
using Peerly.Core.Pagination;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchTeacherCourseHomeworks;

public sealed record SearchTeacherCourseHomeworksQuery : IQuery<SearchTeacherCourseHomeworksQueryResponse>
{
    public required TeacherId TeacherId { get; init; }
    public required CourseId CourseId { get; init; }
    public required SearchCourseHomeworksQueryFilter Filter { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
}
