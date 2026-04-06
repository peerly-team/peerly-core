using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.AddGroupParticipant;

public sealed record AddGroupParticipantCommand : ICommand<Success>
{
    public required GroupId GroupId { get; init; }
    public required StudentId StudentId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
