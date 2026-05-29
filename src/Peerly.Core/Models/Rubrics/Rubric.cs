using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Rubrics;

public sealed record Rubric
{
    public required RubricId Id { get; init; }
    public required TeacherId TeacherId { get; init; }
    public required string Name { get; init; }
}
