using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Teachers;

public sealed record Teacher
{
    public required TeacherId Id { get; init; }
    public required string Email { get; init; }
    public required string? Name { get; init; }
}
