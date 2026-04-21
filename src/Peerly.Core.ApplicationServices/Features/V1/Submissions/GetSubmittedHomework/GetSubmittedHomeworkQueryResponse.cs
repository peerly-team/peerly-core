using System.Collections.Generic;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetSubmittedHomework;

public sealed record GetSubmittedHomeworkQueryResponse
{
    public required SubmittedHomework Submission { get; init; }
    public required IReadOnlyCollection<File> Files { get; init; }
    public required IReadOnlyCollection<SubmittedReview> Reviews { get; init; }
    public required int? FinalMark { get; init; }
}
