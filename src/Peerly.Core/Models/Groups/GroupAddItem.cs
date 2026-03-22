using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Groups;

public sealed record GroupAddItem
{
    public required CourseId CourseId { get; init; }
    public required string Name { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
