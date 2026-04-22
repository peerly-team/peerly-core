namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworks.Models;

internal sealed record SubmittedHomeworkDb
{
    public required long Id { get; init; }
    public required long HomeworkId { get; init; }
    public required long StudentId { get; init; }
    public required string Comment { get; init; }
}
