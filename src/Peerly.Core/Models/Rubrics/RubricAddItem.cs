using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Rubrics;

public sealed record RubricAddItem
{
    public required TeacherId TeacherId { get; init; }
    public required string Name { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
