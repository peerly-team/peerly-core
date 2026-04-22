namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworks.Models;

internal sealed record SubmittedHomeworkStudentDb
{
    public required long Id { get; init; }
    public required long StudentId { get; init; }
}
