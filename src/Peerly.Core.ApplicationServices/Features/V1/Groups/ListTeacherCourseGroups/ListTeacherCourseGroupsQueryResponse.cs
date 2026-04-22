using System.Collections.Generic;
using Peerly.Core.Models.Groups;

namespace Peerly.Core.ApplicationServices.Features.V1.Groups.ListTeacherCourseGroups;

public sealed record ListTeacherCourseGroupsQueryResponse
{
    public required IReadOnlyCollection<Group> Groups { get; init; }
}
