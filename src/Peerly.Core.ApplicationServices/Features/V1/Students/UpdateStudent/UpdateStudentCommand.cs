using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Students.UpdateStudent;

public sealed record UpdateStudentCommand : ICommand<Success>
{
    public required StudentId StudentId { get; init; }
    public required string Name { get; init; }
}
