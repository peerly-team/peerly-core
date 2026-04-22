namespace Peerly.Core.Persistence.Repositories.Students.Models;

internal sealed record StudentDb
{
    public required long Id { get; init; }
    public required string Email { get; init; }
    public required string? Name { get; init; }
}
