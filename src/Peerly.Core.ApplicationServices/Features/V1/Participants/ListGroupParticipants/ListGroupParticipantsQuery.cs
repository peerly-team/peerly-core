using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.ListGroupParticipants;

public sealed record ListGroupParticipantsQuery : IQuery<ListGroupParticipantsQueryResponse>
{
    public required GroupId GroupId { get; init; }
}
