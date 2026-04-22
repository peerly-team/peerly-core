using System.Collections.Generic;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.ListStudentCourseGroups;

public sealed record ListStudentCourseGroupsQueryResponse
{
    public required IReadOnlyCollection<Group> Groups { get; init; }
}
