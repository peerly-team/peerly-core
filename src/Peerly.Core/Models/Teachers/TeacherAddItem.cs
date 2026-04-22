using System;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Teachers;

public sealed record TeacherAddItem
{
    public required TeacherId Id { get; init; }
    public required string Email { get; init; }
    public required string? Name { get; init; }
    public required DateTimeOffset CreationTime { get; init; }
}
