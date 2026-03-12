using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.CreateHomeworkSubmission;

public sealed record CreateHomeworkSubmissionCommandResponse
{
    public required HomeworkSubmissionId HomeworkSubmissionId { get; init; }
}
