using Peerly.Core.ApplicationServices.Abstractions;
using Peerly.Core.Identifiers;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.SearchCourseGroups;

public sealed record SearchCourseGroupsQuery : IQuery<SearchCourseGroupsQueryResponse>
{
    public required CourseId CourseId { get; init; }
    public required TeacherId TeacherId { get; init; }
}
