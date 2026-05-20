using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;
using Peerly.Core.Pagination;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.SearchStudentHomeworks;

public sealed record SearchStudentHomeworksQuery : IQuery<SearchStudentHomeworksQueryResponse>
{
    public required StudentId StudentId { get; init; }
    public required SearchStudentHomeworksQueryFilter Filter { get; init; }
    public required PaginationInfo PaginationInfo { get; init; }
}
