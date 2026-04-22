namespace Peerly.Core.Persistence.Repositories.GroupTeachers.Models;

internal sealed record GroupTeacherDb
{
    public required long GroupId { get; init; }
    public required long TeacherId { get; init; }
}
