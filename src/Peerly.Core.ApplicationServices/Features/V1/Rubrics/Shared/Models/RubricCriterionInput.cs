namespace Peerly.Core.ApplicationServices.Features.V1.Rubrics.Shared.Models;

public sealed record RubricCriterionInput
{
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required int MaxScore { get; init; }
    public required bool CommentRequired { get; init; }
    public required int Position { get; init; }
}
