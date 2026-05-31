using System;
using System.Collections.Generic;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Groups;

public sealed record GroupStudentBulkAddItem
{
    public required GroupId GroupId { get; init; }
    public required IReadOnlyCollection<StudentId> StudentIds { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
