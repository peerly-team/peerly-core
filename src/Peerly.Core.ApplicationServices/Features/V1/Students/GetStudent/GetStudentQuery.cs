using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Students.GetStudent;

public sealed record GetStudentQuery : IQuery<GetStudentQueryResponse>
{
    public required StudentId StudentId { get; init; }
}
