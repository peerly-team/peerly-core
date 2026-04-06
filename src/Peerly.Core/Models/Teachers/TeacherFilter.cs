using System.Collections.Generic;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Teachers;

public sealed record TeacherFilter
{
    public required IReadOnlyCollection<TeacherId> TeacherIds { get; init; }
}
