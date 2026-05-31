using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Rubrics;

public sealed record RubricCriterion
{
    public required RubricCriterionId Id { get; init; }
    public required RubricId RubricId { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required int MaxScore { get; init; }
    public required bool CommentRequired { get; init; }
    public required int Position { get; init; }
}
