namespace Peerly.Core.Persistence.Repositories.Rubrics.Models;

internal sealed record RubricDb
{
    public required long Id { get; init; }
    public required long TeacherId { get; init; }
    public required string Name { get; init; }
}
