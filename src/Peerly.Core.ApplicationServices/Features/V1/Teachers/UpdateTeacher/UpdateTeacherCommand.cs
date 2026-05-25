using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Teachers.UpdateTeacher;

public sealed record UpdateTeacherCommand : ICommand<Success>
{
    public required TeacherId TeacherId { get; init; }
    public required string Name { get; init; }
}
