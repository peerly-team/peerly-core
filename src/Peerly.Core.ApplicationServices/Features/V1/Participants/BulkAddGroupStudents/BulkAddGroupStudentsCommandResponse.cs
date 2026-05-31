using System.Collections.Generic;
using Peerly.Core.Identifiers;
using Peerly.Core.Models.Participants;

namespace Peerly.Core.ApplicationServices.Features.V1.Participants.BulkAddGroupStudents;

public sealed record BulkAddGroupStudentsCommandResponse
{
    public required IReadOnlyCollection<StudentId> AddedStudentIds { get; init; }
    public required IReadOnlyCollection<SkippedStudentInfo> SkippedStudents { get; init; }
}
