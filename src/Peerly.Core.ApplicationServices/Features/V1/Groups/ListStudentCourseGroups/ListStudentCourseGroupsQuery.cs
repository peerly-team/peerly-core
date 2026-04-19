using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.ListStudentCourseGroups;

public sealed record ListStudentCourseGroupsQuery : IQuery<ListStudentCourseGroupsQueryResponse>
{
    public required StudentId StudentId { get; init; }
    public required CourseId CourseId { get; init; }
}
