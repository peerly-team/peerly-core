using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmissionDetail;

public sealed record GetTeacherSubmissionDetailQuery : IQuery<GetTeacherSubmissionDetailQueryResponse>
{
    public required SubmittedHomeworkId SubmittedHomeworkId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
