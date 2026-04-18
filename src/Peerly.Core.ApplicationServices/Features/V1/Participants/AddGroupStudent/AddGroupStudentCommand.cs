using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupStudent;

public sealed record AddGroupStudentCommand : ICommand<Success>
{
    public required GroupId GroupId { get; init; }
    public required StudentId StudentId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
