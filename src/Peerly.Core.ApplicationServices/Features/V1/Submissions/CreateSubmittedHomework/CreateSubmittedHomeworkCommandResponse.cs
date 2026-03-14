using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateSubmittedHomework;

public sealed record CreateSubmittedHomeworkCommandResponse
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
}
