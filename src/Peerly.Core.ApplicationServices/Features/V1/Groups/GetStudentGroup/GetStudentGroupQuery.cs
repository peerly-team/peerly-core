using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.GetStudentGroup;

public sealed record GetStudentGroupQuery : IQuery<GetStudentGroupQueryResponse>
{
    public required GroupId GroupId { get; init; }
    public required StudentId StudentId { get; init; }
}
