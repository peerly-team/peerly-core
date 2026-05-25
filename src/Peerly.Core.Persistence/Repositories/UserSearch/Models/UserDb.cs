namespace Peerly.Core.Persistence.Repositories.UserSearch.Models;

internal sealed record UserDb
{
    public required long Id { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
}
