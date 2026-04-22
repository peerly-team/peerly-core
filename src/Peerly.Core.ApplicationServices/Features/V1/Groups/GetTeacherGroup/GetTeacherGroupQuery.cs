using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.GetTeacherGroup;

public sealed record GetTeacherGroupQuery : IQuery<GetTeacherGroupQueryResponse>
{
    public required GroupId GroupId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
