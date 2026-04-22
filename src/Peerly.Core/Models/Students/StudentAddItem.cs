using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Students;

public sealed record StudentAddItem
{
    public required StudentId Id { get; init; }
    public required string Email { get; init; }
    public required string? Name { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
