using System.Collections.Generic;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Students;

public sealed record StudentFilter
{
    public required IReadOnlyCollection<StudentId> StudentIds { get; init; }
}
