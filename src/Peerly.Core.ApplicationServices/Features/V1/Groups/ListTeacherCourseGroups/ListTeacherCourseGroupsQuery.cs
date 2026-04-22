using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.ListTeacherCourseGroups;

public sealed record ListTeacherCourseGroupsQuery : IQuery<ListTeacherCourseGroupsQueryResponse>
{
    public required TeacherId TeacherId { get; init; }
    public required CourseId CourseId { get; init; }
}
