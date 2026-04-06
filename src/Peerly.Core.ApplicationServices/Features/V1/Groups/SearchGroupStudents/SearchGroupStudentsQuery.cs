using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.SearchGroupStudents;

public sealed record SearchGroupStudentsQuery : IQuery<SearchGroupStudentsQueryResponse>
{
    public required GroupId GroupId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
