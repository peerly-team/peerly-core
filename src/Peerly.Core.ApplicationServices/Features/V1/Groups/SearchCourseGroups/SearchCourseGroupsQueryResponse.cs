using System.Collections.Generic;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.SearchCourseGroups;

public sealed record SearchCourseGroupsQueryResponse
{
    public required IReadOnlyCollection<Group> Groups { get; init; }
}
