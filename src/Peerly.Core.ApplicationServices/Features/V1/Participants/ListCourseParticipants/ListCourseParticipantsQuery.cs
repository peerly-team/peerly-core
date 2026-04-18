using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.ListCourseParticipants;

public sealed record ListCourseParticipantsQuery : IQuery<ListCourseParticipantsQueryResponse>
{
    public required CourseId CourseId { get; init; }
}
