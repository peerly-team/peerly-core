using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetStudentSubmission;

public sealed record GetStudentSubmissionQuery : IQuery<GetStudentSubmissionQueryResponse>
{
    public required HomeworkId HomeworkId { get; init; }
    public required StudentId StudentId { get; init; }
}
