using OneOf.Types;
using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.DeleteSubmittedHomeworkFile;

public sealed record DeleteSubmittedHomeworkFileCommand : ICommand<Success>
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required FileId FileId { get; init; }
    public required StudentId StudentId { get; init; }
}
