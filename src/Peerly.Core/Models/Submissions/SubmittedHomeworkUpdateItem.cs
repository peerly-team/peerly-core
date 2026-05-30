namespace Peerly.Core.Models.Submissions;

public sealed record SubmittedHomeworkUpdateItem
{
    public required string? Comment { get; init; }
}
