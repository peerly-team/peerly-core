using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomeworkFile;

public sealed record CreateSubmittedHomeworkFileCommandResponse
{
    public required FileId FileId { get; init; }
}
