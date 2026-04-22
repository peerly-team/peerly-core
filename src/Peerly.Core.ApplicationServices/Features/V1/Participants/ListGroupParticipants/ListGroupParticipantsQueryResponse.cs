using System.Collections.Generic;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Teachers;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.ListGroupParticipants;

public sealed record ListGroupParticipantsQueryResponse
{
    public required IReadOnlyCollection<Teacher> Teachers { get; init; }
    public required IReadOnlyCollection<Student> Students { get; init; }
}
