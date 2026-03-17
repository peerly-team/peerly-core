namespace Peerly.Core.Persistence.Repositories.GroupStudents.Models;

internal sealed record GroupStudentDb
{
    public required long GroupId { get; init; }
    public required long StudentId { get; init; }
}
