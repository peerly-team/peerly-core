using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Pagination;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchTeacherHomeworks;

public sealed record SearchTeacherHomeworksQuery : IQuery<SearchTeacherHomeworksQueryResponse>
{
    public required TeacherId TeacherId { get; init; }
    public required SearchTeacherHomeworksQueryFilter Filter { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
}
