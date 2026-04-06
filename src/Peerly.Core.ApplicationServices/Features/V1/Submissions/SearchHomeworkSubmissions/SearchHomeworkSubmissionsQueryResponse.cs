using System.Collections.Generic;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.SearchHomeworkSubmissions;

public sealed record SearchHomeworkSubmissionsQueryResponse
{
    public required IReadOnlyCollection<SubmissionOverviewItem> Submissions { get; init; }
}

public sealed record SubmissionOverviewItem
{
    public required long SubmittedHomeworkId { get; init; }
    public required long StudentId { get; init; }
    public required string StudentName { get; init; }
    public required int? ReviewersMark { get; init; }
    public required int? TeacherMark { get; init; }
    public required bool HasDiscrepancy { get; init; }
    public required int ReviewsReceived { get; init; }
}
