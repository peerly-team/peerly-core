using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;

public sealed record CreateSubmittedHomeworkFileCommand : ICommand<CreateSubmittedHomeworkFileCommandResponse>
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required StorageId StorageId { get; init; }
    public required string FileName { get; init; }
    public required int FileSize { get; init; }
    public required StudentId StudentId { get; init; }
}
