namespace Peerly.Core.Models.Users;

public sealed record User
{
    public required long Id { get; init; }
    public required string Email { get; init; }
    public required string? Name { get; init; }
    public required UserRole Role { get; init; }
}
