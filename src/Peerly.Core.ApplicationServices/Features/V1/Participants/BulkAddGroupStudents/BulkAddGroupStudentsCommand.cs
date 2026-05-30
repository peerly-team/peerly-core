using System.Collections.Generic;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.BulkAddGroupStudents;

public sealed record BulkAddGroupStudentsCommand : ICommand<BulkAddGroupStudentsCommandResponse>
{
    public required GroupId GroupId { get; init; }
    public required IReadOnlyCollection<StudentId> StudentIds { get; init; }
    public required TeacherId TeacherId { get; init; }
}
