using System.Collections.Generic;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmissionDetail;

public sealed record GetTeacherSubmissionDetailQueryResponse
{
    public required long SubmittedHomeworkId { get; init; }
    public required long StudentId { get; init; }
    public required string StudentName { get; init; }
    public required string Comment { get; init; }
    public required IReadOnlyCollection<File> Files { get; init; }
    public required IReadOnlyCollection<ReviewWithStudent> Reviews { get; init; }
    public required SubmittedHomeworkMark? Mark { get; init; }
}

public sealed record ReviewWithStudent
{
    public required SubmittedReview Review { get; init; }
    public required Student? Reviewer { get; init; }
}
