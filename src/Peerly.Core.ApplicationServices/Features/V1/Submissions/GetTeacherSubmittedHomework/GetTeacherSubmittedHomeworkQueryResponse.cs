using System.Collections.Generic;
using Peerly.Core.Models.Files;
using Peerly.Core.Models.Students;
using Peerly.Core.Models.Submissions;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetTeacherSubmittedHomework;

public sealed record GetTeacherSubmittedHomeworkQueryResponse
{
    public required SubmittedHomework SubmittedHomework { get; init; }
    public required Student Student { get; init; }
    public required IReadOnlyCollection<File> Files { get; init; }
    public required IReadOnlyCollection<TeacherSubmittedReview> SubmittedReviews { get; init; }
    public required int ReviewersMark { get; init; }
    public required int? TeacherMark { get; init; }
}
