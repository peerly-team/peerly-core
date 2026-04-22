namespace Peerly.Core.Persistence.Repositories.Teachers.Models;

internal sealed record TeacherDb
{
    public required long Id { get; init; }
    public required string Email { get; init; }
    public required string? Name { get; init; }
}
