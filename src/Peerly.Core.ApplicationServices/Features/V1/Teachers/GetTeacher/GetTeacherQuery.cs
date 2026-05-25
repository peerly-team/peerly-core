using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Teachers.GetTeacher;

public sealed record GetTeacherQuery : IQuery<GetTeacherQueryResponse>
{
    public required TeacherId TeacherId { get; init; }
}
