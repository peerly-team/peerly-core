using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Students;

public sealed record Student
{
    public required StudentId Id { get; init; }
    public required string Email { get; init; }
    public required string? Name { get; init; }
}
