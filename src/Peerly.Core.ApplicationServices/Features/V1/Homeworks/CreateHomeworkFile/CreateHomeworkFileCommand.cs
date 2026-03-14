using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Homeworks.CreateHomeworkFile;

public sealed record CreateHomeworkFileCommand : ICommand<CreateHomeworkFileCommandResponse>
{
    public required HomeworkId HomeworkId { get; init; }
    public required StorageId StorageId { get; init; }
    public required string FileName { get; init; }
    public required int FileSize { get; init; }
    public required TeacherId TeacherId { get; init; }
}
