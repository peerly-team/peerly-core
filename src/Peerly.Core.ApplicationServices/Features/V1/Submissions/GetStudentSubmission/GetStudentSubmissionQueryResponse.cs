using System.Collections.Generic;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetStudentSubmission;

public sealed record GetStudentSubmissionQueryResponse
{
    public required long SubmittedHomeworkId { get; init; }
    public required string Comment { get; init; }
    public required IReadOnlyCollection<File> Files { get; init; }
    public required IReadOnlyCollection<SubmittedReview> Reviews { get; init; }
    public required SubmittedHomeworkMark? Mark { get; init; }
}
