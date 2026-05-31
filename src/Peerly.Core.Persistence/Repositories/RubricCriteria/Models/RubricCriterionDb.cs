namespace Peerly.Core.Persistence.Repositories.RubricCriteria.Models;

internal sealed record RubricCriterionDb
{
    public required long Id { get; init; }
    public required long RubricId { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required int MaxScore { get; init; }
    public required bool CommentRequired { get; init; }
    public required int Position { get; init; }
}
