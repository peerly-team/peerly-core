using System.Collections.Generic;
using Peerly.Core.Identifiers;

namespace Peerly.Core.Models.Groups;

public sealed record GroupFilter
{
    public required IReadOnlyCollection<GroupId> GroupIds { get; init; }
    public required IReadOnlyCollection<CourseId> CourseIds { get; init; }

    public static GroupFilter Empty()
    {
        return new GroupFilter
        {
            GroupIds = [],
            CourseIds = []
        };
    }
}
