using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Rubrics;

public sealed record RubricCriterionAddItem
{
    public required RubricId RubricId { get; init; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required int MaxScore { get; init; }
    public required bool CommentRequired { get; init; }
    public required int Position { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
