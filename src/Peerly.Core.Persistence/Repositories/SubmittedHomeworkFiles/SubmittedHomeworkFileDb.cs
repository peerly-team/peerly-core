namespace Peerly.Core.Persistence.Repositories.SubmittedHomeworkFiles;

internal sealed record SubmittedHomeworkFileDb
{
    public required long FileId { get; init; }
    public required long? AnonymizedFileId { get; init; }
}
