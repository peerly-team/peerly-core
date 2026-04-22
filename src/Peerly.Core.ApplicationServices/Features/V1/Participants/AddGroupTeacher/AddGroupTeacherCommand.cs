using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.AddGroupTeacher;

public sealed record AddGroupTeacherCommand : ICommand<Success>
{
    public required GroupId GroupId { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required TeacherId ActorTeacherId { get; init; }
}
