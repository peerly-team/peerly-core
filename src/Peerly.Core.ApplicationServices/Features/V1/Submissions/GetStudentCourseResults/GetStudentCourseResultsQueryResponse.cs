using System.Collections.Generic;
using Peerly.Core.Models.Homeworks;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetStudentCourseResults;

public sealed record GetStudentCourseResultsQueryResponse
{
    public required IReadOnlyCollection<StudentHomeworkResultItem> Results { get; init; }
}

public sealed record StudentHomeworkResultItem
{
    public required long HomeworkId { get; init; }
    public required string HomeworkName { get; init; }
    public required HomeworkStatus HomeworkStatus { get; init; }
    public required int? ReviewersMark { get; init; }
    public required int? TeacherMark { get; init; }
}
