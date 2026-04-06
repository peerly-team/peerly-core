using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Submissions.GetStudentCourseResults;

public sealed record GetStudentCourseResultsQuery : IQuery<GetStudentCourseResultsQueryResponse>
{
    public required StudentId StudentId { get; init; }
    public required CourseId CourseId { get; init; }
}
